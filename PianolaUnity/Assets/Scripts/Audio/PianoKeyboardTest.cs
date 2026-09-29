using UnityEngine;
using UnityEngine.InputSystem;

// Plays the SfizzLivePlayer from the computer keyboard, DAW-style:
// A W S E D F T G Y H U J K = C to C, Z/X = octave down/up, Space = sustain pedal.
public class PianoKeyboardTest : MonoBehaviour
{
    public SfizzLivePlayer Player;
    [Range(1, 127)]
    public int Velocity = 90;
    public int Octave = 4;

    static readonly Key[] keys =
    {
        Key.A, Key.W, Key.S, Key.E, Key.D, Key.F, Key.T, Key.G, Key.Y, Key.H, Key.U, Key.J, Key.K
    };

    // Remember which note each key started, so changing octave mid-press still releases the right note.
    readonly int[] heldNotes = new int[keys.Length];

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || Player == null || !Player.IsLoaded)
            return;

        if (keyboard.zKey.wasPressedThisFrame)
            Octave = Mathf.Max(0, Octave - 1);
        if (keyboard.xKey.wasPressedThisFrame)
            Octave = Mathf.Min(8, Octave + 1);

        if (keyboard.spaceKey.wasPressedThisFrame)
            Player.Sustain(true);
        if (keyboard.spaceKey.wasReleasedThisFrame)
            Player.Sustain(false);

        for (int i = 0; i < keys.Length; i++)
        {
            var key = keyboard[keys[i]];

            if (key.wasPressedThisFrame)
            {
                heldNotes[i] = Mathf.Clamp((Octave + 1) * 12 + i, 0, 127);
                Player.NoteOn(heldNotes[i], Velocity);
            }
            if (key.wasReleasedThisFrame)
                Player.NoteOff(heldNotes[i]);
        }
    }
}
