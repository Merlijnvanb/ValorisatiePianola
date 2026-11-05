using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

// Shows an arrow at every screen edge/corner that leads to a neighboring CameraPoint right now.
// Arrows brighten as the mouse nears their edge. Builds its own arrows: put it on a Canvas and go.
[RequireComponent(typeof(RectTransform))]
public class EdgeIndicators : MonoBehaviour
{
    public CameraController CameraController;

    [Tooltip("Distance from the screen edge, in canvas pixels")]
    public float Margin = 45f;
    public float Size = 36f;
    public Color Color = new Color(1f, 0.9f, 0.75f);
    [Range(0, 1)] public float IdleAlpha = 0.3f;
    [Tooltip("Mouse distance from the center (0..1) where an arrow starts to brighten")]
    [Range(0, 1)] public float HighlightStart = 0.6f;
    public float FadeSpeed = 10f;

    // All 8 directions a Neighbor.Transition can have.
    static readonly int2[] directions =
    {
        new int2(0, 1), new int2(1, 1), new int2(1, 0), new int2(1, -1),
        new int2(0, -1), new int2(-1, -1), new int2(-1, 0), new int2(-1, 1),
    };

    Image[] arrows;
    bool[] available;

    void Awake()
    {
        if (!CameraController)
            CameraController = FindFirstObjectByType<CameraController>();

        arrows = new Image[directions.Length];
        available = new bool[directions.Length];

        for (int i = 0; i < directions.Length; i++)
        {
            var d = directions[i];
            arrows[i] = UIArrow.Create(transform, $"PointArrow {d.x},{d.y}", new Vector2(d.x, d.y), Margin, Size, Color);
        }
    }

    void OnEnable() => CameraController.OnPointChanged += HandlePointChanged;
    void OnDisable() => CameraController.OnPointChanged -= HandlePointChanged;

    // CameraController.Start may have fired OnPointChanged before we subscribed.
    void Start()
    {
        if (CameraController.CurrentPoint)
            HandlePointChanged(CameraController.CurrentPoint);
    }

    void HandlePointChanged(CameraPoint point)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            available[i] = false;
            foreach (var neighbor in point.Neighbors)
            {
                if (neighbor.Point && neighbor.Transition.Equals(directions[i]))
                    available[i] = true;
            }
        }
    }

    void Update()
    {
        var mouse = UIArrow.RelativeMouse();
        float threshold = CameraController.TransitionThreshold;

        for (int i = 0; i < directions.Length; i++)
        {
            var d = new Vector2(directions[i].x, directions[i].y);
            float proximity = available[i] ? UIArrow.Proximity(d, mouse, HighlightStart, threshold) : 0f;
            float alpha = available[i] ? Mathf.Lerp(IdleAlpha, 1f, proximity) : 0f;
            UIArrow.Animate(arrows[i], alpha, Mathf.Lerp(1f, 1.35f, proximity), FadeSpeed);
        }
    }
}
