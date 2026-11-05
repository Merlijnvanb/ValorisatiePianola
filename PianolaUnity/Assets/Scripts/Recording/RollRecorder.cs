using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Opnemen-flow: the "Opnemen" button flies to the recording piano, Start/Afronden records what is
// played on the keyboard, then a prompt asks for piece/composer/performer and the roll goes in the closet.
public class RollRecorder : MonoBehaviour
{
    public CameraController CameraController;
    public CameraPoint RecordPoint;
    public CameraPoint RollsPoint;
    public CameraPoint ReturnPoint;
    public RollsManager RollsManager;
    public MidiSequencer MidiSequencer;
    public PianoKeyboardTest Keyboard;

    [Header("UI")]
    public Button OpnemenButton;
    public GameObject RecordPanel;
    public Button StartButton;
    public Button FinishButton;
    public Button BackButton;
    public TextMeshProUGUI StatusText;

    [Header("Prompt")]
    public GameObject PromptPanel;
    public TMP_InputField PieceInput;
    public TMP_InputField ComposerInput;
    public TMP_InputField PerformerInput;
    public Button SaveButton;
    public Button DiscardButton;

    enum State { Idle, AtPiano, Recording, Naming }

    State state;
    double startTime;
    readonly List<RecordedRolls.RecordedEvent> events = new List<RecordedRolls.RecordedEvent>();
    readonly HashSet<int> soundingNotes = new HashSet<int>();
    bool sustainDown;

    void Awake()
    {
        OpnemenButton.onClick.AddListener(GoToPiano);
        StartButton.onClick.AddListener(StartRecording);
        FinishButton.onClick.AddListener(FinishRecording);
        BackButton.onClick.AddListener(Leave);
        SaveButton.onClick.AddListener(Save);
        DiscardButton.onClick.AddListener(Discard);
    }

    void OnEnable()
    {
        CameraController.OnPointChanged += HandlePointChanged;
        Keyboard.NotePressed += HandleNotePressed;
        Keyboard.NoteReleased += HandleNoteReleased;
        Keyboard.SustainChanged += HandleSustainChanged;
    }

    void OnDisable()
    {
        CameraController.OnPointChanged -= HandlePointChanged;
        Keyboard.NotePressed -= HandleNotePressed;
        Keyboard.NoteReleased -= HandleNoteReleased;
        Keyboard.SustainChanged -= HandleSustainChanged;
    }

    void Start() => SetState(State.Idle);

    void Update()
    {
        if (state == State.Recording)
            StatusText.SetText($"<color=#e04040>●</color> Opname {FormatTime(Now)}   ({CountNotes()} noten)");
    }

    // --- Buttons ---

    void GoToPiano()
    {
        if (!CameraController.ChangePoint(RecordPoint))
            return;

        // The pianola would otherwise keep playing through the recording.
        MidiSequencer.Stop();
    }

    void StartRecording()
    {
        events.Clear();
        soundingNotes.Clear();
        startTime = Time.unscaledTimeAsDouble;

        // Keys and pedal already held when Start is pressed are part of the recording too.
        if (sustainDown)
            Add(RecordedRolls.EventType.ControlChange, 64, 127);

        SetState(State.Recording);
    }

    void FinishRecording()
    {
        // Close anything still held so the roll never ends with a hanging note.
        double end = Now;
        foreach (int note in soundingNotes)
            events.Add(new RecordedRolls.RecordedEvent { Time = end, Type = RecordedRolls.EventType.NoteOff, A = note });
        soundingNotes.Clear();
        if (sustainDown)
            events.Add(new RecordedRolls.RecordedEvent { Time = end, Type = RecordedRolls.EventType.ControlChange, A = 64, B = 0 });

        if (CountNotes() == 0)
        {
            SetState(State.AtPiano);
            StatusText.SetText("Niets opgenomen. Druk op Start en speel iets.");
            return;
        }

        TrimLeadingSilence();
        SetState(State.Naming);
    }

    void Leave()
    {
        if (state == State.Recording)
            return;
        CameraController.ChangePoint(ReturnPoint);
    }

    void Save()
    {
        string piece = Clean(PieceInput.text, "Zonder titel");
        string composer = Clean(ComposerInput.text, "Onbekend");
        string performer = Clean(PerformerInput.text, "Onbekend");

        var data = RecordedRolls.Save(events, piece, composer, performer);
        events.Clear();

        if (!RollsManager.AddRoll(data))
            Debug.LogWarning("The rolls closet is full; the recording was saved but is not in the closet this session.");

        SetState(State.Idle);
        CameraController.ChangePoint(RollsPoint);
    }

    void Discard()
    {
        events.Clear();
        SetState(State.AtPiano);
    }

    // --- Keyboard input ---

    void HandleNotePressed(int note, int velocity)
    {
        if (state != State.Recording)
            return;
        soundingNotes.Add(note);
        Add(RecordedRolls.EventType.NoteOn, note, velocity);
    }

    void HandleNoteReleased(int note)
    {
        // A note released after Start but pressed before it has no note-on, so it is skipped.
        if (state != State.Recording || !soundingNotes.Remove(note))
            return;
        Add(RecordedRolls.EventType.NoteOff, note, 0);
    }

    void HandleSustainChanged(bool down)
    {
        sustainDown = down;
        if (state == State.Recording)
            Add(RecordedRolls.EventType.ControlChange, 64, down ? 127 : 0);
    }

    // --- Helpers ---

    void HandlePointChanged(CameraPoint point)
    {
        if (point == RecordPoint)
        {
            if (state == State.Idle)
                SetState(State.AtPiano);
        }
        else if (state != State.Idle)
        {
            // Left the piano some other way: throw away an unfinished take.
            events.Clear();
            SetState(State.Idle);
        }
    }

    void SetState(State newState)
    {
        state = newState;

        OpnemenButton.gameObject.SetActive(state == State.Idle);
        RecordPanel.SetActive(state == State.AtPiano || state == State.Recording);
        PromptPanel.SetActive(state == State.Naming);

        StartButton.interactable = state == State.AtPiano;
        FinishButton.interactable = state == State.Recording;
        BackButton.interactable = state == State.AtPiano;

        // Typing a title would otherwise play the piano.
        Keyboard.enabled = state != State.Naming;

        switch (state)
        {
            case State.AtPiano:
                StatusText.SetText("Druk op Start en speel.  (A–K = toetsen, Z/X = octaaf, spatie = pedaal)");
                break;
            case State.Naming:
                PieceInput.text = ComposerInput.text = PerformerInput.text = "";
                PieceInput.Select();
                PieceInput.ActivateInputField();
                break;
        }
    }

    double Now => Time.unscaledTimeAsDouble - startTime;

    void Add(RecordedRolls.EventType type, int a, int b) =>
        events.Add(new RecordedRolls.RecordedEvent { Time = Now, Type = type, A = a, B = b });

    // The roll starts at the first note instead of at the moment Start was pressed.
    void TrimLeadingSilence()
    {
        double first = double.MaxValue;
        foreach (var e in events)
            if (e.Type == RecordedRolls.EventType.NoteOn && e.Time < first)
                first = e.Time;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            e.Time = System.Math.Max(0, e.Time - first);
            events[i] = e;
        }
    }

    int CountNotes()
    {
        int count = 0;
        foreach (var e in events)
            if (e.Type == RecordedRolls.EventType.NoteOn)
                count++;
        return count;
    }

    static string FormatTime(double seconds) => $"{(int)(seconds / 60)}:{(int)(seconds % 60):00}";

    static string Clean(string text, string fallback) =>
        string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();
}
