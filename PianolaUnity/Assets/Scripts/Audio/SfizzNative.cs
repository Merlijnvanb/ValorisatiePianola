using System;
using System.Runtime.InteropServices;
using System.Text;

// Minimal bindings to the sfizz C API (see sfizz.h, tag 1.2.3).
// Thread rules from sfizz.h: "CT/OFF" functions must not run while the audio thread is inside an "RT" function.
public static class SfizzNative
{
    const string Lib = "sfizz";

    // CT/OFF
    [DllImport(Lib)] public static extern IntPtr sfizz_create_synth();
    [DllImport(Lib)] public static extern void sfizz_free(IntPtr synth);
    [DllImport(Lib)] public static extern void sfizz_set_sample_rate(IntPtr synth, float sampleRate);
    [DllImport(Lib)] public static extern void sfizz_set_samples_per_block(IntPtr synth, int samplesPerBlock);
    [DllImport(Lib)] public static extern void sfizz_set_num_voices(IntPtr synth, int numVoices);

    [DllImport(Lib)]
    [return: MarshalAs(UnmanagedType.I1)]
    static extern bool sfizz_load_file(IntPtr synth, byte[] path);

    public static bool LoadFile(IntPtr synth, string path)
    {
        return sfizz_load_file(synth, Encoding.UTF8.GetBytes(path + '\0'));
    }

    // RT
    [DllImport(Lib)] public static extern void sfizz_send_note_on(IntPtr synth, int delay, int noteNumber, int velocity);
    [DllImport(Lib)] public static extern void sfizz_send_note_off(IntPtr synth, int delay, int noteNumber, int velocity);
    [DllImport(Lib)] public static extern void sfizz_send_cc(IntPtr synth, int delay, int ccNumber, int ccValue);
    [DllImport(Lib)] public static extern void sfizz_send_pitch_wheel(IntPtr synth, int delay, int pitch);
    [DllImport(Lib)] public static extern void sfizz_all_sound_off(IntPtr synth);
    [DllImport(Lib)] public static extern int sfizz_get_num_active_voices(IntPtr synth);

    // channels is a float** : a pointer to an array of per-channel float buffers.
    [DllImport(Lib)] public static extern void sfizz_render_block(IntPtr synth, IntPtr channels, int numChannels, int numFrames);
}
