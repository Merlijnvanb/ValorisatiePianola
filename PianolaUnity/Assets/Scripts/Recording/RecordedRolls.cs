using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using UnityEngine;

// Stores recorded rolls in StreamingAssets/Midi/Recordings: a .mid with the music and a .json next
// to it with the RollData fields. At startup every .json there becomes a RollData again, so
// recordings survive restarts (in the editor and in builds).
public static class RecordedRolls
{
    public const string Folder = "Midi/Recordings"; // relative to StreamingAssets

    public enum EventType : byte { NoteOn, NoteOff, ControlChange }

    public struct RecordedEvent
    {
        public double Time; // seconds from the start of the recording
        public EventType Type;
        public int A;
        public int B;
    }

    // 120 bpm at 480 ticks per quarter: one tick is ~1 ms, plenty for frame-polled input.
    const int TicksPerQuarter = 480;
    const long MicrosecondsPerQuarter = 500_000;

    static string FullFolder => Path.Combine(Application.streamingAssetsPath, Folder);

    public static RollData Save(IReadOnlyList<RecordedEvent> events, string piece, string composer, string performer)
    {
        Directory.CreateDirectory(FullFolder);

        string baseName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Sanitize(piece)}";
        string midiPath = $"{Folder}/{baseName}.mid";

        WriteMidi(events, Path.Combine(Application.streamingAssetsPath, midiPath));

        var data = ScriptableObject.CreateInstance<RollData>();
        data.name = baseName;
        data.MidiPath = midiPath;
        data.PieceTitle = piece;
        data.ComposerName = composer;
        data.PerformerName = performer;

        File.WriteAllText(Path.Combine(FullFolder, baseName + ".json"), JsonUtility.ToJson(data, true));
        return data;
    }

    public static List<RollData> LoadAll()
    {
        var result = new List<RollData>();
        if (!Directory.Exists(FullFolder))
            return result;

        // File names start with a timestamp, so this is oldest first.
        foreach (var jsonPath in Directory.GetFiles(FullFolder, "*.json").OrderBy(p => p))
        {
            try
            {
                var data = ScriptableObject.CreateInstance<RollData>();
                JsonUtility.FromJsonOverwrite(File.ReadAllText(jsonPath), data);
                data.name = Path.GetFileNameWithoutExtension(jsonPath);

                if (File.Exists(Path.Combine(Application.streamingAssetsPath, data.MidiPath)))
                    result.Add(data);
                else
                    Debug.LogWarning($"Recording '{jsonPath}' points to a missing MIDI file '{data.MidiPath}'");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not load recording '{jsonPath}': {e.Message}");
            }
        }

        return result;
    }

    static void WriteMidi(IReadOnlyList<RecordedEvent> events, string fullPath)
    {
        var track = new TrackChunk();
        track.Events.Add(new SetTempoEvent(MicrosecondsPerQuarter));

        long lastTick = 0;
        foreach (var e in events)
        {
            long tick = (long)Math.Round(e.Time * 1e6 / MicrosecondsPerQuarter * TicksPerQuarter);

            MidiEvent midiEvent = e.Type switch
            {
                EventType.NoteOn => new NoteOnEvent((SevenBitNumber)(byte)e.A, (SevenBitNumber)(byte)e.B),
                EventType.NoteOff => new NoteOffEvent((SevenBitNumber)(byte)e.A, (SevenBitNumber)(byte)0),
                _ => new ControlChangeEvent((SevenBitNumber)(byte)e.A, (SevenBitNumber)(byte)e.B),
            };
            midiEvent.DeltaTime = Math.Max(0, tick - lastTick);
            lastTick = Math.Max(lastTick, tick);

            track.Events.Add(midiEvent);
        }

        var file = new MidiFile(track) { TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerQuarter) };
        file.Write(fullPath, true);
    }

    static string Sanitize(string name)
    {
        string cleaned = Regex.Replace(name ?? "", @"[^A-Za-z0-9]+", "_").Trim('_');
        if (cleaned.Length > 40)
            cleaned = cleaned.Substring(0, 40);
        return cleaned.Length > 0 ? cleaned : "Opname";
    }
}
