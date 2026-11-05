using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Pianola > Setup Internals Prototype: fills the pianola shell with placeholder internals (Welte-Mignon
// chain), adds a camera point per station chained left/right, links them to the pianola view, gives the
// shell the cutaway material and adds an ObstructionController. Run once, then tweak by hand.
public static class InternalsSetup
{
    const string CameraPointPrefab = "Assets/Prefabs/CameraPoint.prefab";
    const string MaterialFolder = "Assets/Materials/Internals";
    const string ShellMaterialPath = "Assets/Materials/PianolaShell.mat";
    const float HoleRadius = 0.4f;
    const float CameraDistance = 0.9f; // from the shell surface

    // Placement inside the shell. Pos is a fraction of the shell (-0.5..0.5) along
    // (across, height, depth from front to back); Size is in meters along the same axes.
    struct Part
    {
        public string Name;
        public PrimitiveType Type;
        public Vector3 Pos;
        public Vector3 Size;
        public string Material;
        public bool LyingAcross; // cylinders: axis along "across" instead of upright
    }

    struct Station
    {
        public string Name;
        public Vector3 Focus; // same fractions as Part.Pos
        public Part[] Parts;
    }

    static Station[] Stations()
    {
        var stack = new List<Part>();
        for (int i = 0; i < 10; i++)
        {
            float a = Mathf.Lerp(-0.4f, 0.4f, i / 9f);
            stack.Add(new Part { Name = $"Pneumatiek {i + 1}", Type = PrimitiveType.Cube, Pos = new Vector3(a, 0.12f, 0.2f), Size = new Vector3(0.12f, 0.3f, 0.15f), Material = "Leer" });
        }
        stack.Add(new Part { Name = "Windlade", Type = PrimitiveType.Cube, Pos = new Vector3(0, -0.1f, 0.2f), Size = new Vector3(1.7f, 0.08f, 0.22f), Material = "Hout" });

        return new[]
        {
            new Station
            {
                Name = "Rol & notenbalk", Focus = new Vector3(0, 0.22f, -0.3f),
                Parts = new[]
                {
                    new Part { Name = "Rol", Type = PrimitiveType.Cylinder, Pos = new Vector3(0, 0.3f, -0.3f), Size = new Vector3(0.7f, 0.12f, 0.12f), Material = "Rol", LyingAcross = true },
                    new Part { Name = "Opwikkelrol", Type = PrimitiveType.Cylinder, Pos = new Vector3(0, 0.1f, -0.3f), Size = new Vector3(0.7f, 0.16f, 0.16f), Material = "Hout", LyingAcross = true },
                    new Part { Name = "Notenbalk", Type = PrimitiveType.Cube, Pos = new Vector3(0, 0.2f, -0.37f), Size = new Vector3(0.8f, 0.03f, 0.04f), Material = "Messing" },
                },
            },
            new Station
            {
                Name = "Stack (pneumatiek)", Focus = new Vector3(0, 0.05f, 0.2f),
                Parts = stack.ToArray(),
            },
            new Station
            {
                Name = "Luchtmotor & governor", Focus = new Vector3(0.33f, -0.05f, -0.1f),
                Parts = new[]
                {
                    new Part { Name = "Luchtmotor", Type = PrimitiveType.Cube, Pos = new Vector3(0.33f, -0.2f, -0.1f), Size = new Vector3(0.3f, 0.25f, 0.25f), Material = "Leer" },
                    new Part { Name = "Governor", Type = PrimitiveType.Cube, Pos = new Vector3(0.33f, 0.08f, -0.1f), Size = new Vector3(0.15f, 0.15f, 0.15f), Material = "Hout" },
                    new Part { Name = "Tempoklep", Type = PrimitiveType.Cylinder, Pos = new Vector3(0.43f, 0.08f, -0.1f), Size = new Vector3(0.06f, 0.06f, 0.06f), Material = "Messing" },
                },
            },
            new Station
            {
                Name = "Pomp & reservoir", Focus = new Vector3(-0.3f, -0.3f, -0.1f),
                Parts = new[]
                {
                    new Part { Name = "Pomp", Type = PrimitiveType.Cylinder, Pos = new Vector3(-0.3f, -0.3f, 0.1f), Size = new Vector3(0.25f, 0.25f, 0.25f), Material = "Messing" },
                    new Part { Name = "Reservoir", Type = PrimitiveType.Cube, Pos = new Vector3(-0.3f, -0.32f, -0.25f), Size = new Vector3(0.4f, 0.2f, 0.35f), Material = "Leer" },
                },
            },
        };
    }

    static readonly Dictionary<string, (Color color, float smoothness, float metallic)> materialLooks = new()
    {
        { "Hout", (new Color(0.30f, 0.18f, 0.09f), 0.35f, 0f) },
        { "Leer", (new Color(0.32f, 0.22f, 0.15f), 0.2f, 0f) },
        { "Messing", (new Color(0.78f, 0.6f, 0.3f), 0.65f, 1f) },
        { "Rol", (new Color(0.6f, 0.14f, 0.11f), 0.25f, 0f) }, // red Welte-Mignon paper
    };

    [MenuItem("Pianola/Setup Internals Prototype")]
    static void Setup()
    {
        var rollsManager = Object.FindFirstObjectByType<RollsManager>();
        var cameraController = Object.FindFirstObjectByType<CameraController>();
        var sequencer = Object.FindFirstObjectByType<MidiSequencer>();
        if (!rollsManager || !cameraController || !sequencer)
        {
            EditorUtility.DisplayDialog("Internals setup", "Needs a RollsManager, CameraController and MidiSequencer (on the pianola) in the open scene.", "OK");
            return;
        }

        var pianola = sequencer.transform;
        if (pianola.Find("Internals"))
        {
            EditorUtility.DisplayDialog("Internals setup", "The pianola already has an 'Internals' child.", "OK");
            return;
        }

        var shell = pianola.GetComponentInChildren<MeshRenderer>();
        if (!shell)
        {
            EditorUtility.DisplayDialog("Internals setup", $"No MeshRenderer (shell) found under '{pianola.name}'.", "OK");
            return;
        }

        var pianolaPoint = rollsManager.PianolaPoint;

        Undo.SetCurrentGroupName("Setup Internals Prototype");
        int undoGroup = Undo.GetCurrentGroup();

        // --- Shell frame: which local axes are across / up / back, seen from the pianola view ---
        var shellT = shell.transform;
        var toCamLocal = shellT.InverseTransformDirection(pianolaPoint.transform.position - shellT.position);
        toCamLocal.y = 0;
        Vector3 frontAxis = Mathf.Abs(toCamLocal.x) > Mathf.Abs(toCamLocal.z)
            ? new Vector3(Mathf.Sign(toCamLocal.x), 0, 0)
            : new Vector3(0, 0, Mathf.Sign(toCamLocal.z));
        Vector3 backAxis = -frontAxis;
        Vector3 acrossAxis = Vector3.Cross(Vector3.up, backAxis); // camera's right, in shell space

        // Shell size in meters along across/up/back, and helpers to go to world space.
        Vector3 scale = shellT.lossyScale;
        Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        float across = Vector3.Dot(Abs(acrossAxis), scale);
        float height = scale.y;
        float depth = Vector3.Dot(Abs(backAxis), scale);
        var shellCenter = shell.bounds.center;

        // Directions in world space (unscaled).
        var acrossW = shellT.TransformDirection(acrossAxis).normalized;
        var upW = shellT.TransformDirection(Vector3.up).normalized;
        var backW = shellT.TransformDirection(backAxis).normalized;
        Vector3 ToWorld(Vector3 f) => shellCenter + acrossW * (f.x * across) + upW * (f.y * height) + backW * (f.z * depth);
        var partRotation = Quaternion.LookRotation(backW, upW); // local x = across, y = up, z = back

        // --- Materials ---
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/Materials", "Internals");
        var lit = Shader.Find("Universal Render Pipeline/Lit");
        var materials = new Dictionary<string, Material>();
        foreach (var look in materialLooks)
        {
            string path = $"{MaterialFolder}/{look.Key}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat)
            {
                mat = new Material(lit);
                mat.SetColor("_BaseColor", look.Value.color);
                mat.SetFloat("_Smoothness", look.Value.smoothness);
                mat.SetFloat("_Metallic", look.Value.metallic);
                AssetDatabase.CreateAsset(mat, path);
            }
            materials[look.Key] = mat;
        }

        // --- Internals ---
        var internals = new GameObject("Internals");
        Undo.RegisterCreatedObjectUndo(internals, "Create internals");
        internals.transform.SetParent(pianola, false);

        var light = new GameObject("InternalsLight").AddComponent<Light>();
        light.transform.SetParent(internals.transform, false);
        light.transform.position = ToWorld(new Vector3(0, 0.35f, -0.2f));
        light.type = LightType.Point;
        light.range = Mathf.Max(across, depth) * 0.9f;
        light.intensity = 0.6f;
        light.color = new Color(1f, 0.8f, 0.55f);

        var stations = Stations();
        var stationPoints = new List<(CameraPoint point, float screenX)>();
        var viewDir = (pianolaPoint.transform.position - pianolaPoint.Target.position).normalized; // towards camera
        var camRight = Vector3.Cross(Vector3.up, -viewDir).normalized;
        var pointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CameraPointPrefab);
        var bounds = shell.bounds;

        foreach (var station in stations)
        {
            var group = new GameObject(station.Name);
            group.transform.SetParent(internals.transform, false);

            foreach (var part in station.Parts)
            {
                var go = GameObject.CreatePrimitive(part.Type);
                go.name = part.Name;
                go.transform.SetParent(group.transform, true);
                go.transform.position = ToWorld(part.Pos);
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.GetComponent<MeshRenderer>().sharedMaterial = materials[part.Material];

                if (part.Type == PrimitiveType.Cylinder)
                {
                    // Unity's cylinder: axis along local y, 2 units long, 1 wide.
                    bool lying = part.LyingAcross;
                    go.transform.rotation = lying ? partRotation * Quaternion.Euler(0, 0, 90) : partRotation;
                    float length = lying ? part.Size.x : part.Size.y;
                    float diameter = lying ? part.Size.y : part.Size.x;
                    go.transform.localScale = new Vector3(diameter, length / 2f, diameter);
                }
                else
                {
                    go.transform.rotation = partRotation;
                    go.transform.localScale = part.Size;
                }
            }

            // Camera point: outside the shell on the camera side, looking at the station like the pianola view does.
            var focus = ToWorld(station.Focus);
            var probe = new Ray(focus + viewDir * 50f, -viewDir);
            var surface = bounds.IntersectRay(probe, out float hit) ? probe.GetPoint(hit) : focus + viewDir;

            var pointGo = (GameObject)PrefabUtility.InstantiatePrefab(pointPrefab);
            pointGo.name = $"Point {station.Name}";
            pointGo.transform.SetParent(group.transform, true);
            pointGo.transform.position = surface + viewDir * CameraDistance;
            var point = pointGo.GetComponent<CameraPoint>();
            point.Target.position = focus;
            PrefabUtility.RecordPrefabInstancePropertyModifications(pointGo.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(point.Target);

            stationPoints.Add((point, Vector3.Dot(focus, camRight)));
        }

        // --- Navigation: stations left/right in screen order, entered from and left back to the pianola view ---
        var ordered = stationPoints.OrderBy(s => s.screenX).Select(s => s.point).ToList();
        var used = pianolaPoint.Neighbors?.Select(n => n.Transition).ToList() ?? new List<int2>();
        var enter = used.Contains(new int2(0, -1)) && !used.Contains(new int2(0, 1)) ? new int2(0, 1) : new int2(0, -1);

        for (int i = 0; i < ordered.Count; i++)
        {
            var neighbors = new List<CameraPoint.Neighbor> { new CameraPoint.Neighbor { Point = pianolaPoint, Transition = -enter } };
            if (i > 0)
                neighbors.Add(new CameraPoint.Neighbor { Point = ordered[i - 1], Transition = new int2(-1, 0) });
            if (i < ordered.Count - 1)
                neighbors.Add(new CameraPoint.Neighbor { Point = ordered[i + 1], Transition = new int2(1, 0) });
            ordered[i].Neighbors = neighbors.ToArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(ordered[i]);
        }

        // Enter at the station in the middle of the screen (the roll / stack area).
        var entry = ordered[ordered.Count / 2];
        Undo.RecordObject(pianolaPoint, "Link internals");
        var pianolaNeighbors = (pianolaPoint.Neighbors ?? new CameraPoint.Neighbor[0]).Where(n => !n.Transition.Equals(enter)).ToList();
        pianolaNeighbors.Add(new CameraPoint.Neighbor { Point = entry, Transition = enter });
        pianolaPoint.Neighbors = pianolaNeighbors.ToArray();
        PrefabUtility.RecordPrefabInstancePropertyModifications(pianolaPoint);

        // --- Shell: cutaway material, colored like the old one ---
        var shellMat = AssetDatabase.LoadAssetAtPath<Material>(ShellMaterialPath);
        if (!shellMat)
        {
            shellMat = new Material(Shader.Find("Pianola/Cutaway Lit"));
            var old = shell.sharedMaterial;
            if (old && old.HasProperty("_Albedo"))
                shellMat.SetColor("_BaseColor", old.GetColor("_Albedo"));
            else if (old && old.HasProperty("_BaseColor"))
                shellMat.SetColor("_BaseColor", old.GetColor("_BaseColor"));
            AssetDatabase.CreateAsset(shellMat, ShellMaterialPath);
        }
        Undo.RecordObject(shell, "Shell cutaway material");
        shell.sharedMaterial = shellMat;

        // --- Logic ---
        var controllerGo = new GameObject("ObstructionController");
        Undo.RegisterCreatedObjectUndo(controllerGo, "Create obstruction controller");
        var logic = GameObject.Find("-- LOGIC");
        if (logic)
            controllerGo.transform.SetParent(logic.transform, false);
        var controller = controllerGo.AddComponent<ObstructionController>();
        controller.CameraController = cameraController;
        controller.Points = ordered.Select(p => new ObstructionController.CutawayPoint { Point = p, Radius = HoleRadius }).ToArray();

        AssetDatabase.SaveAssets();
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(internals.scene);
        Selection.activeGameObject = internals;
        Debug.Log($"Internals prototype set up: {ordered.Count} stations, entered from '{pianolaPoint.name}' with transition {enter}.");
    }
}
