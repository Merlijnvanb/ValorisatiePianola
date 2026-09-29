using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

// Plays a MIDI file through a SfizzLivePlayer. Each frame it hands the player the events of the
// next Lookahead seconds, stamped with their exact sample time on the player's clock. So timing
// is sample-accurate no matter the frame rate, as long as a frame never takes longer than Lookahead.
public class MidiSequencer : MonoBehaviour
{
    public SfizzLivePlayer Player;
    [Tooltip("Path to the .mid file, relative to StreamingAssets")]
    public string MidiPath = "Midi/arabesque_1_(c)oguri.mid";
    public bool PlayOnLoad = true;
    [Range(0.25f, 4f)]
    public float Speed = 1f;
    [Range(-24, 24)]
    public int Transpose;
    [Tooltip("How far ahead (seconds) events are sent to the audio thread. Must cover the longest frame hitch.")]
    [Range(0.05f, 1f)]
    public float Lookahead = 0.2f;

    public bool IsPlaying => playing;
    public double Length { get; private set; }
    // Song position (seconds) of what is being rendered right now. Use this to sync visuals.
    public double SongTime => playing ? Math.Max(0, SampleToTime(Player.CurrentSample)) : stoppedTime;

    enum SequencedEventType : byte { NoteOn, NoteOff, ControlChange }

    struct SequencedEvent
    {
        public double Time;
        public SequencedEventType Type;
        public int A;
        public int B;
    }

    List<SequencedEvent> events = new List<SequencedEvent>();
    int nextEvent;
    bool playing;
    bool playRequested;
    double stoppedTime;

    // Maps song time to the player's sample clock: sample = anchorSample + (time - anchorTime) * rate / speed.
    long anchorSample;
    double anchorTime;
    float anchorSpeed = 1f;
    long scheduledUntil;

    // Note actually sent to the player for each MIDI note, so Transpose changes never leave notes hanging.
    readonly int[] soundingNotes = new int[128];

    void Awake()
    {
        Array.Fill(soundingNotes, -1);
        Load(MidiPath);
        playRequested = PlayOnLoad;
    }

    void OnDisable()
    {
        if (playing)
            Stop();
    }

    public void Load(string midiPath)
    {
        if (playing)
            Stop();

        MidiPath = midiPath;
        var file = MidiFile.Read(Path.Combine(Application.streamingAssetsPath, midiPath));
        var tempoMap = file.GetTempoMap();
        var loaded = new List<SequencedEvent>();

        // All tracks and channels go to the one piano.
        foreach (var timedEvent in file.GetTimedEvents())
        {
            double time = timedEvent.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 1e6;

            switch (timedEvent.Event)
            {
                // A note-on with velocity 0 is a note-off by MIDI convention.
                case NoteOnEvent noteOn when noteOn.Velocity > 0:
                    loaded.Add(new SequencedEvent { Time = time, Type = SequencedEventType.NoteOn, A = noteOn.NoteNumber, B = noteOn.Velocity });
                    break;
                case NoteOnEvent noteOn:
                    loaded.Add(new SequencedEvent { Time = time, Type = SequencedEventType.NoteOff, A = noteOn.NoteNumber });
                    break;
                case NoteOffEvent noteOff:
                    loaded.Add(new SequencedEvent { Time = time, Type = SequencedEventType.NoteOff, A = noteOff.NoteNumber, B = noteOff.Velocity });
                    break;
                case ControlChangeEvent cc:
                    loaded.Add(new SequencedEvent { Time = time, Type = SequencedEventType.ControlChange, A = cc.ControlNumber, B = cc.ControlValue });
                    break;
            }
        }

        // Stable sort; at equal times release before striking so repeated notes re-trigger.
        events = loaded.OrderBy(e => e.Time).ThenBy(e => e.Type == SequencedEventType.NoteOff ? 0 : 1).ToList();
        Length = events.Count > 0 ? events[events.Count - 1].Time : 0;
        stoppedTime = 0;
    }

    [ContextMenu("Play")]
    public void Play() => Play(0);

    public void Play(double fromTime)
    {
        if (!Player.IsLoaded)
        {
            // Started from Update once the piano has finished loading.
            playRequested = true;
            stoppedTime = fromTime;
            return;
        }

        if (playing)
            Stop();

        // Start one lookahead in the future so the first events are not late.
        anchorSample = Player.CurrentSample + SecondsToSamples(Lookahead);
        anchorTime = fromTime;
        anchorSpeed = Speed;
        scheduledUntil = anchorSample;

        // Skip to fromTime, but restore the pedals/controllers as they were at that point.
        var lastControlValues = new Dictionary<int, int>();
        nextEvent = 0;
        while (nextEvent < events.Count && events[nextEvent].Time < fromTime)
        {
            var e = events[nextEvent++];
            if (e.Type == SequencedEventType.ControlChange)
                lastControlValues[e.A] = e.B;
        }
        foreach (var control in lastControlValues)
            Player.ControlChange(control.Key, control.Value, anchorSample);

        playing = true;
    }

    [ContextMenu("Pause")]
    public void Pause()
    {
        if (playing)
            Stop();
    }

    [ContextMenu("Resume")]
    public void Resume() => Play(stoppedTime);

    [ContextMenu("Stop")]
    public void Stop()
    {
        stoppedTime = SongTime;
        playing = false;
        playRequested = false;

        if (Player == null)
            return;

        Player.ClearScheduled();
        for (int i = 0; i < soundingNotes.Length; i++)
        {
            if (soundingNotes[i] < 0)
                continue;
            Player.NoteOff(soundingNotes[i]);
            soundingNotes[i] = -1;
        }
        Player.Sustain(false);
    }

    void Update()
    {
        if (!playing)
        {
            if (playRequested && Player.IsLoaded)
            {
                playRequested = false;
                Play(stoppedTime);
            }
            return;
        }

        if (Speed != anchorSpeed)
            Reanchor();

        long horizon = Player.CurrentSample + SecondsToSamples(Lookahead);
        while (nextEvent < events.Count)
        {
            var e = events[nextEvent];
            long sample = TimeToSample(e.Time);
            if (sample >= horizon)
                break;

            Send(e, sample);
            nextEvent++;
        }
        scheduledUntil = Math.Max(scheduledUntil, horizon);

        if (nextEvent >= events.Count && SampleToTime(Player.CurrentSample) >= Length)
        {
            stoppedTime = Length;
            playing = false;
        }
    }

    void Send(SequencedEvent e, long sample)
    {
        switch (e.Type)
        {
            case SequencedEventType.NoteOn:
                int note = Mathf.Clamp(e.A + Transpose, 0, 127);
                soundingNotes[e.A] = note;
                Player.NoteOn(note, e.B, sample);
                break;
            case SequencedEventType.NoteOff:
                int sounding = soundingNotes[e.A] >= 0 ? soundingNotes[e.A] : Mathf.Clamp(e.A + Transpose, 0, 127);
                soundingNotes[e.A] = -1;
                Player.NoteOff(sounding, e.B, sample);
                break;
            case SequencedEventType.ControlChange:
                Player.ControlChange(e.A, e.B, sample);
                break;
        }
    }

    // Events up to scheduledUntil are already with the player at the old speed, so the new speed
    // takes over from there. A speed change is therefore heard Lookahead seconds later.
    void Reanchor()
    {
        anchorTime = SampleToTime(scheduledUntil);
        anchorSample = scheduledUntil;
        anchorSpeed = Mathf.Max(Speed, 0.01f);
    }

    long TimeToSample(double time) => anchorSample + (long)Math.Round((time - anchorTime) * Player.SampleRate / anchorSpeed);
    double SampleToTime(long sample) => anchorTime + (sample - anchorSample) * (double)anchorSpeed / Player.SampleRate;
    long SecondsToSamples(float seconds) => (long)(seconds * Player.SampleRate);
}
