using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Pianola > Setup Recording Prototype: builds the placeholder recording piano, its camera point and the
// Opnemen UI in the open scene, and wires everything into a RollRecorder. Run it once, then tweak by hand.
public static class RecordingSetup
{
    const string CameraPointPrefab = "Assets/Prefabs/CameraPoint.prefab";

    [MenuItem("Pianola/Setup Recording Prototype")]
    static void Setup()
    {
        var rollsManager = Object.FindFirstObjectByType<RollsManager>();
        var cameraController = Object.FindFirstObjectByType<CameraController>();
        var sequencer = Object.FindFirstObjectByType<MidiSequencer>();
        var keyboard = Object.FindFirstObjectByType<PianoKeyboardTest>();

        if (!rollsManager || !cameraController || !sequencer || !keyboard)
        {
            EditorUtility.DisplayDialog("Recording setup",
                "Needs a RollsManager, CameraController, MidiSequencer and PianoKeyboardTest in the open scene.", "OK");
            return;
        }

        if (Object.FindFirstObjectByType<RollRecorder>())
        {
            EditorUtility.DisplayDialog("Recording setup", "There is already a RollRecorder in the scene.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Setup Recording Prototype");
        int undoGroup = Undo.GetCurrentGroup();

        // --- World: placeholder piano next to the pianola, seen the same way the pianola is seen ---
        var pianolaPoint = rollsManager.PianolaPoint;
        var view = pianolaPoint.GetViewVector();
        var side = Vector3.Cross(Vector3.up, view).normalized;
        const float offset = 3f;

        var root = new GameObject("RecordingPiano");
        Undo.RegisterCreatedObjectUndo(root, "Create recording piano");
        root.transform.position = pianolaPoint.Target.position + side * offset;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "PianoPlaceholder";
        cube.transform.SetParent(root.transform, false);
        cube.transform.localScale = new Vector3(1.5f, 1f, 0.6f);
        cube.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-view, Vector3.up));

        var lightGo = new GameObject("RecordingLight");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localPosition = new Vector3(0, 2f, 0);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 5f;
        light.intensity = 3f;
        light.color = new Color(1f, 0.85f, 0.65f);

        var pointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CameraPointPrefab);
        var pointGo = (GameObject)PrefabUtility.InstantiatePrefab(pointPrefab);
        pointGo.name = "RecordPoint";
        pointGo.transform.SetParent(root.transform, true);
        pointGo.transform.position = pianolaPoint.transform.position + side * offset;
        var recordPoint = pointGo.GetComponent<CameraPoint>();
        recordPoint.Target.position = root.transform.position;
        recordPoint.Neighbors = new CameraPoint.Neighbor[0];
        PrefabUtility.RecordPrefabInstancePropertyModifications(pointGo.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(recordPoint.Target);
        PrefabUtility.RecordPrefabInstancePropertyModifications(recordPoint);

        // --- UI ---
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (!canvas)
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create canvas");
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (!Object.FindFirstObjectByType<EventSystem>())
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create event system");
        }

        var ui = new GameObject("RecordingUI", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(ui, "Create recording UI");
        ui.transform.SetParent(canvas.transform, false);
        Stretch(ui);

        // Kept away from the screen edges, which move the camera.
        var opnemen = MakeButton(ui.transform, "Opnemen");
        Place(opnemen, new Vector2(1, 1), new Vector2(-200, -110), new Vector2(220, 64));

        var recordPanel = MakePanel(ui.transform, "RecordPanel", new Color(0, 0, 0, 0.6f));
        Place(recordPanel, new Vector2(0.5f, 0), new Vector2(0, 160), new Vector2(900, 150));
        var status = MakeText(recordPanel.transform, "Status", "", 24);
        Place(status, new Vector2(0.5f, 1), new Vector2(0, -35), new Vector2(860, 50));
        var start = MakeButton(recordPanel.transform, "Start");
        Place(start, new Vector2(0.5f, 0), new Vector2(-220, 50), new Vector2(200, 56));
        var finish = MakeButton(recordPanel.transform, "Afronden");
        Place(finish, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(200, 56));
        var back = MakeButton(recordPanel.transform, "Terug");
        Place(back, new Vector2(0.5f, 0), new Vector2(220, 50), new Vector2(200, 56));

        var promptPanel = MakePanel(ui.transform, "PromptPanel", new Color(0, 0, 0, 0.85f));
        Place(promptPanel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 460));
        var title = MakeText(promptPanel.transform, "Title", "Nieuwe rol", 36);
        Place(title, new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(640, 60));
        var piece = MakeInput(promptPanel.transform, "Stuk");
        Place(piece, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(600, 56));
        var composer = MakeInput(promptPanel.transform, "Componist");
        Place(composer, new Vector2(0.5f, 1), new Vector2(0, -200), new Vector2(600, 56));
        var performer = MakeInput(promptPanel.transform, "Uitvoerder");
        Place(performer, new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(600, 56));
        var save = MakeButton(promptPanel.transform, "Opslaan");
        Place(save, new Vector2(0.5f, 0), new Vector2(-120, 60), new Vector2(220, 56));
        var discard = MakeButton(promptPanel.transform, "Weggooien");
        Place(discard, new Vector2(0.5f, 0), new Vector2(120, 60), new Vector2(220, 56));

        recordPanel.SetActive(false);
        promptPanel.SetActive(false);

        // --- Logic ---
        var recorderGo = new GameObject("RollRecorder");
        Undo.RegisterCreatedObjectUndo(recorderGo, "Create roll recorder");
        var logic = GameObject.Find("-- LOGIC");
        if (logic)
            recorderGo.transform.SetParent(logic.transform, false);

        var recorder = recorderGo.AddComponent<RollRecorder>();
        recorder.CameraController = cameraController;
        recorder.RecordPoint = recordPoint;
        recorder.RollsPoint = rollsManager.RollsPoint;
        recorder.ReturnPoint = cameraController.StartPoint;
        recorder.RollsManager = rollsManager;
        recorder.MidiSequencer = sequencer;
        recorder.Keyboard = keyboard;
        recorder.OpnemenButton = opnemen.GetComponent<Button>();
        recorder.RecordPanel = recordPanel;
        recorder.StartButton = start.GetComponent<Button>();
        recorder.FinishButton = finish.GetComponent<Button>();
        recorder.BackButton = back.GetComponent<Button>();
        recorder.StatusText = status.GetComponent<TextMeshProUGUI>();
        recorder.PromptPanel = promptPanel;
        recorder.PieceInput = piece.GetComponent<TMP_InputField>();
        recorder.ComposerInput = composer.GetComponent<TMP_InputField>();
        recorder.PerformerInput = performer.GetComponent<TMP_InputField>();
        recorder.SaveButton = save.GetComponent<Button>();
        recorder.DiscardButton = discard.GetComponent<Button>();

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(recorderGo.scene);
        Selection.activeGameObject = root;
        Debug.Log("Recording prototype set up. Move 'RecordingPiano' wherever you like; its RecordPoint moves with it.");
    }

    static GameObject MakeButton(Transform parent, string label)
    {
        var go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        go.name = label + "Button";
        go.transform.SetParent(parent, false);
        var text = go.GetComponentInChildren<TextMeshProUGUI>();
        text.SetText(label);
        text.fontSize = 26;
        return go;
    }

    static GameObject MakeInput(Transform parent, string placeholder)
    {
        var go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
        go.name = placeholder + "Input";
        go.transform.SetParent(parent, false);
        var input = go.GetComponent<TMP_InputField>();
        input.pointSize = 24;
        if (input.placeholder is TMP_Text placeholderText)
            placeholderText.SetText(placeholder);
        return go;
    }

    static GameObject MakePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static GameObject MakeText(Transform parent, string name, string text, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.SetText(text);
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    // Anchors and pivots at the same point, so position is the offset from that anchor.
    static void Place(GameObject go, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    static void Stretch(GameObject go)
    {
        var rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
