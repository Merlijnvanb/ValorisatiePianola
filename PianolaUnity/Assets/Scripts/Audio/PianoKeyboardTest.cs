using System;
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

    // Raised for everything played, so a RollRecorder can listen in.
    public event Action<int, int> NotePressed;   // note, velocity
    public event Action<int> NoteReleased;       // note
    public event Action<bool> SustainChanged;    // pedal down

    static readonly Key[] keys =
    {
        Key.A, Key.W, Key.S, Key.E, Key.D, Key.F, Key.T, Key.G, Key.Y, Key.H, Key.U, Key.J, Key.K
    };

    // Remember which note each key started, so changing octave mid-press still releases the right note.
    readonly int[] heldNotes = new int[keys.Length];
    readonly bool[] keyDown = new bool[keys.Length];
    bool sustainDown;

    // Disabled while typing in a text field: release everything so nothing hangs.
    void OnDisable()
    {
        if (Player == null)
            return;

        for (int i = 0; i < keys.Length; i++)
        {
            if (!keyDown[i])
                continue;
            keyDown[i] = false;
            Player.NoteOff(heldNotes[i]);
            NoteReleased?.Invoke(heldNotes[i]);
        }

        if (sustainDown)
        {
            sustainDown = false;
            Player.Sustain(false);
            SustainChanged?.Invoke(false);
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || Player == null || !Player.IsLoaded)
            return;

        if (keyboard.zKey.wasPressedThisFrame)
            Octave = Mathf.Max(0, Octave - 1);
        if (keyboard.xKey.wasPressedThisFrame)
            Octave = Mathf.Min(8, Octave + 1);

        if (keyboard.spaceKey.wasPressedThisFrame && !sustainDown)
        {
            sustainDown = true;
            Player.Sustain(true);
            SustainChanged?.Invoke(true);
        }
        if (keyboard.spaceKey.wasReleasedThisFrame && sustainDown)
        {
            sustainDown = false;
            Player.Sustain(false);
            SustainChanged?.Invoke(false);
        }

        for (int i = 0; i < keys.Length; i++)
        {
            var key = keyboard[keys[i]];

            if (key.wasPressedThisFrame && !keyDown[i])
            {
                keyDown[i] = true;
                heldNotes[i] = Mathf.Clamp((Octave + 1) * 12 + i, 0, 127);
                Player.NoteOn(heldNotes[i], Velocity);
                NotePressed?.Invoke(heldNotes[i], Velocity);
            }
            if (key.wasReleasedThisFrame && keyDown[i])
            {
                keyDown[i] = false;
                Player.NoteOff(heldNotes[i]);
                NoteReleased?.Invoke(heldNotes[i]);
            }
        }
    }
}
