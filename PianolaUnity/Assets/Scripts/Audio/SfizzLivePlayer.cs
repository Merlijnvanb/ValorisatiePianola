using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Renders sfizz live on Unity's audio thread. Events are queued from the main thread and
// placed sample-accurately inside the audio block they fall in.
// Put this on a GameObject with an AudioSource that has no clip, and keep it the first filter.
[RequireComponent(typeof(AudioSource))]
public class SfizzLivePlayer : MonoBehaviour
{
    public const long Now = -1;

    [Tooltip("Path to the .sfz file, relative to StreamingAssets")]
    public string SfzPath = "SFZ/VCSL_Keys/Upright Piano, Knight.sfz";
    [Range(1, 256)]
    public int NumVoices = 128;

    [Header("Loudness")]
    [Tooltip("Measure the instrument on load and adjust its gain so every instrument plays at TargetLoudnessDb")]
    public bool Normalize = true;
    [Tooltip("Loudness (RMS, dBFS) that a test note at velocity 100 is brought to")]
    [Range(-40, 0)]
    public float TargetLoudnessDb = -20f;
    [Tooltip("Manual trim on top of normalization")]
    [Range(-24, 24)]
    public float VolumeDb;

    public bool IsLoaded => loaded;
    // Loudness of the test notes as the instrument came out of the box; NaN until measured.
    public float MeasuredLoudnessDb => measuredLoudnessDb;
    public int SampleRate { get; private set; }
    // Number of samples rendered so far. This is the clock to schedule events against.
    public long CurrentSample => Interlocked.Read(ref sampleClock);
    public int DroppedEvents => droppedEvents;

    enum SynthEventType : byte { NoteOn, NoteOff, ControlChange, PitchWheel, AllSoundOff, ClearScheduled }

    struct SynthEvent
    {
        public long Sample;
        public SynthEventType Type;
        public int A;
        public int B;
    }

    const int QueueCapacity = 4096; // must be a power of two
    const float MaxNormalizeDb = 24f;
    static readonly int[] loudnessTestNotes = { 48, 60, 72 };
    const int LoudnessTestVelocity = 100;
    const float LoudnessTestSeconds = 0.5f;

    IntPtr synth;
    readonly object synthLock = new object();
    volatile bool loaded;
    volatile bool destroyed;
    int blockSize;
    long sampleClock;
    int droppedEvents;
    volatile float measuredLoudnessDb = float.NaN;
    volatile float targetGain = 1f; // set on the main thread, ramped to on the audio thread
    float currentGain = 1f;

    // Main thread -> audio thread. Single producer (main thread), single consumer (audio thread).
    readonly SynthEvent[] queue = new SynthEvent[QueueCapacity];
    int queueWrite;
    int queueRead;

    // Audio thread only, kept sorted by Sample.
    readonly SynthEvent[] pending = new SynthEvent[QueueCapacity];
    int pendingCount;

    float[] left;
    float[] right;
    GCHandle leftHandle;
    GCHandle rightHandle;
    GCHandle channelsHandle;
    IntPtr channelsPtr;

    void Awake()
    {
        SampleRate = AudioSettings.outputSampleRate;
        AudioSettings.GetDSPBufferSize(out blockSize, out _);

        // Pinned once so rendering never allocates on the audio thread.
        left = new float[blockSize];
        right = new float[blockSize];
        leftHandle = GCHandle.Alloc(left, GCHandleType.Pinned);
        rightHandle = GCHandle.Alloc(right, GCHandleType.Pinned);
        var channels = new[] { leftHandle.AddrOfPinnedObject(), rightHandle.AddrOfPinnedObject() };
        channelsHandle = GCHandle.Alloc(channels, GCHandleType.Pinned);
        channelsPtr = channelsHandle.AddrOfPinnedObject();

        var path = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, SfzPath));
        int rate = SampleRate, block = blockSize, voices = NumVoices;
        Task.Run(() => Load(path, rate, block, voices));

        var source = GetComponent<AudioSource>();
        source.clip = null;
        if (!source.isPlaying)
            source.Play();
    }

    void Update()
    {
        float gainDb = VolumeDb;
        float measured = measuredLoudnessDb;
        if (Normalize && !float.IsNaN(measured))
            gainDb += Mathf.Clamp(TargetLoudnessDb - measured, -MaxNormalizeDb, MaxNormalizeDb);

        targetGain = Mathf.Pow(10f, gainDb / 20f);
    }

    void OnDestroy()
    {
        destroyed = true;
        loaded = false;

        // Waits for a running render or load to finish before freeing the synth.
        lock (synthLock)
        {
            if (synth != IntPtr.Zero)
                SfizzNative.sfizz_free(synth);
            synth = IntPtr.Zero;
        }

        channelsHandle.Free();
        leftHandle.Free();
        rightHandle.Free();
    }

    // Loading a big SFZ takes seconds, so it runs on a worker thread. The audio thread outputs
    // silence until it is done.
    void Load(string path, int rate, int block, int voices)
    {
        try
        {
            lock (synthLock)
            {
                if (destroyed)
                    return;

                synth = SfizzNative.sfizz_create_synth();
                SfizzNative.sfizz_set_sample_rate(synth, rate);
                SfizzNative.sfizz_set_samples_per_block(synth, block);
                SfizzNative.sfizz_set_num_voices(synth, voices);

                var stopwatch = Stopwatch.StartNew();
                if (!SfizzNative.LoadFile(synth, path))
                {
                    Debug.LogError($"sfizz could not load '{path}'");
                    return;
                }

                long loadMs = stopwatch.ElapsedMilliseconds;
                measuredLoudnessDb = MeasureLoudnessDb(rate, block);
                Debug.Log($"sfizz loaded '{Path.GetFileName(path)}' in {loadMs} ms ({rate} Hz, block {block}), " +
                          $"loudness {measuredLoudnessDb:F1} dB measured in {stopwatch.ElapsedMilliseconds - loadMs} ms");
                loaded = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Renders a few test notes offline and returns their average RMS loudness in dBFS.
    // Runs inside Load while holding synthLock, so the audio thread is not using the synth or buffers.
    float MeasureLoudnessDb(int rate, int block)
    {
        int framesPerNote = (int)(LoudnessTestSeconds * rate);
        double totalRms = 0;
        int audibleNotes = 0;

        SfizzNative.sfizz_enable_freewheeling(synth);

        foreach (int note in loudnessTestNotes)
        {
            SfizzNative.sfizz_send_note_on(synth, 0, note, LoudnessTestVelocity);

            double sumOfSquares = 0;
            for (int rendered = 0; rendered < framesPerNote; rendered += block)
            {
                int count = Math.Min(block, framesPerNote - rendered);
                SfizzNative.sfizz_render_block(synth, channelsPtr, 2, count);
                for (int i = 0; i < count; i++)
                    sumOfSquares += left[i] * left[i] + right[i] * right[i];
            }

            SfizzNative.sfizz_all_sound_off(synth);

            // Notes outside the instrument's range are silent and would drag the average down.
            double rms = Math.Sqrt(sumOfSquares / (framesPerNote * 2));
            if (rms > 1e-5)
            {
                totalRms += rms;
                audibleNotes++;
            }
        }

        SfizzNative.sfizz_disable_freewheeling(synth);

        if (audibleNotes == 0)
            return float.NaN;
        return 20f * (float)Math.Log10(totalRms / audibleNotes);
    }

    // The methods below may only be called from the main thread.
    // atSample is an absolute time on the CurrentSample clock; Now plays in the next audio block.

    public void NoteOn(int note, int velocity, long atSample = Now) => Enqueue(SynthEventType.NoteOn, note, velocity, atSample);
    public void NoteOff(int note, int velocity = 0, long atSample = Now) => Enqueue(SynthEventType.NoteOff, note, velocity, atSample);
    public void ControlChange(int cc, int value, long atSample = Now) => Enqueue(SynthEventType.ControlChange, cc, value, atSample);
    public void PitchWheel(int pitch, long atSample = Now) => Enqueue(SynthEventType.PitchWheel, pitch, 0, atSample);
    public void Sustain(bool down, long atSample = Now) => ControlChange(64, down ? 127 : 0, atSample);
    public void AllSoundOff() => Enqueue(SynthEventType.AllSoundOff, 0, 0, Now);
    // Drops every event sent before this call that has not played yet.
    public void ClearScheduled() => Enqueue(SynthEventType.ClearScheduled, 0, 0, Now);

    void Enqueue(SynthEventType type, int a, int b, long atSample)
    {
        int write = queueWrite;
        if (write - Volatile.Read(ref queueRead) >= QueueCapacity)
        {
            Interlocked.Increment(ref droppedEvents);
            return;
        }

        queue[write & (QueueCapacity - 1)] = new SynthEvent { Sample = atSample, Type = type, A = a, B = b };
        Volatile.Write(ref queueWrite, write + 1);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        // Never block the audio thread: if a load or teardown holds the lock, output silence.
        if (!loaded || !Monitor.TryEnter(synthLock))
        {
            Array.Clear(data, 0, data.Length);
            return;
        }

        try
        {
            if (!loaded)
            {
                Array.Clear(data, 0, data.Length);
                return;
            }

            DrainQueue();

            int frames = data.Length / channels;
            for (int offset = 0; offset < frames; offset += blockSize)
            {
                int count = Math.Min(blockSize, frames - offset);
                long start = sampleClock;

                DispatchEvents(start, count);
                SfizzNative.sfizz_render_block(synth, channelsPtr, 2, count);
                WriteOutput(data, channels, offset, count);

                Interlocked.Exchange(ref sampleClock, start + count);
            }
        }
        finally
        {
            Monitor.Exit(synthLock);
        }
    }

    void DrainQueue()
    {
        int write = Volatile.Read(ref queueWrite);
        while (queueRead != write)
        {
            var e = queue[queueRead & (QueueCapacity - 1)];
            Volatile.Write(ref queueRead, queueRead + 1);

            if (e.Type == SynthEventType.ClearScheduled)
            {
                pendingCount = 0;
                continue;
            }

            if (pendingCount == pending.Length)
            {
                Interlocked.Increment(ref droppedEvents);
                continue;
            }

            // Insertion sort; equal times keep their queue order (note-off before a re-strike, etc).
            int i = pendingCount++;
            while (i > 0 && pending[i - 1].Sample > e.Sample)
            {
                pending[i] = pending[i - 1];
                i--;
            }
            pending[i] = e;
        }
    }

    // Sends every event that falls before the end of this block, with its offset inside the block.
    // sfizz requires events within a block to be in delay order, which the sorted list guarantees.
    void DispatchEvents(long start, int count)
    {
        long end = start + count;
        int sent = 0;

        while (sent < pendingCount && pending[sent].Sample < end)
        {
            var e = pending[sent++];
            int delay = e.Sample <= start ? 0 : (int)(e.Sample - start);

            switch (e.Type)
            {
                case SynthEventType.NoteOn:
                    SfizzNative.sfizz_send_note_on(synth, delay, e.A, e.B);
                    break;
                case SynthEventType.NoteOff:
                    SfizzNative.sfizz_send_note_off(synth, delay, e.A, e.B);
                    break;
                case SynthEventType.ControlChange:
                    SfizzNative.sfizz_send_cc(synth, delay, e.A, e.B);
                    break;
                case SynthEventType.PitchWheel:
                    SfizzNative.sfizz_send_pitch_wheel(synth, delay, e.A);
                    break;
                case SynthEventType.AllSoundOff:
                    SfizzNative.sfizz_all_sound_off(synth);
                    break;
            }
        }

        if (sent > 0)
        {
            pendingCount -= sent;
            Array.Copy(pending, sent, pending, 0, pendingCount);
        }
    }

    // sfizz renders planar stereo; Unity wants interleaved samples with any channel count.
    // Gain changes are ramped across the block so moving the volume never clicks.
    void WriteOutput(float[] data, int channels, int offset, int count)
    {
        float gain = currentGain;
        float gainStep = (targetGain - gain) / count;

        for (int i = 0; i < count; i++)
        {
            gain += gainStep;
            int index = (offset + i) * channels;

            if (channels == 1)
            {
                data[index] = (left[i] + right[i]) * 0.5f * gain;
                continue;
            }

            data[index] = left[i] * gain;
            data[index + 1] = right[i] * gain;
            for (int c = 2; c < channels; c++)
                data[index + c] = 0f;
        }

        currentGain = gain;
    }
}
