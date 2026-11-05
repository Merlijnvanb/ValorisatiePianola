using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

// Navigation hints for the rolls closet, shown only while the closet is active and only for moves
// that are possible right now.
// - Up/down: screen-edge arrows (the mouse at the edge moves), brightening as the mouse nears them.
// - Scrolling: one group in 3D on the closet face, to the left of the selected roll (the right side is
//   covered by the roll's info panel): ◀ mouse ▶. The arrows point along the closet; the mouse wheel rolls
//   the way you can scroll (up = right, down = left, both = back and forth). Until the player has
//   scrolled once this visit the group pulses to draw attention; after that it settles down.
// Builds everything itself: put it on a Canvas and go.
[RequireComponent(typeof(RectTransform))]
public class RollsIndicators : MonoBehaviour
{
    public RollsManager RollsManager;
    public CameraController CameraController;

    [Header("Up / down (screen edge)")]
    public float EdgeMargin = 45f;
    public float EdgeSize = 36f;
    [Range(0, 1)] public float HighlightStart = 0.6f;

    [Header("Scroll hint (3D, world units)")]
    [Tooltip("Distance from the selected roll to the outer (left) arrow, along the closet")]
    public float SideDistance = 0.6f;
    [Tooltip("Spacing between each arrow and the mouse icon")]
    public float IconGap = 0.16f;
    [Tooltip("Lift off the closet face towards the camera, so the hint doesn't clip into it")]
    public float FrontOffset = 0.05f;
    public float ArrowSize = 0.12f;
    public float IconSize = 0.16f;
    [Tooltip("How fast the hint glides to the newly selected roll")]
    public float FollowSpeed = 14f;
    [Tooltip("Wheel rolls per second on the mouse icon")]
    public float WheelSpeed = 1.2f;

    [Header("Look")]
    public Color Color = new Color(1f, 0.9f, 0.75f);
    [Range(0, 1)] public float IdleAlpha = 0.3f;
    public float FadeSpeed = 10f;
    [Tooltip("How fast the flash after a scroll fades")]
    public float PulseDecay = 4f;

    class ScrollArrow
    {
        public int Sign; // +1 = RollsManager "right" (wheel up), -1 = "left" (wheel down)
        public SpriteRenderer Renderer;
        public float Alpha, Pulse;
    }

    Image up, down;
    ScrollArrow[] arrows;
    SpriteRenderer body, wheel;
    float iconAlpha;
    Vector3 position; // the mouse icon, center of the group
    GameObject hintRoot;
    Camera cam;
    bool wasActive;
    bool hasScrolled;

    void Awake()
    {
        if (!RollsManager)
            RollsManager = FindFirstObjectByType<RollsManager>();
        if (!CameraController)
            CameraController = FindFirstObjectByType<CameraController>();

        up = UIArrow.Create(transform, "RollsArrow Up", Vector2.up, EdgeMargin, EdgeSize, Color);
        down = UIArrow.Create(transform, "RollsArrow Down", Vector2.down, EdgeMargin, EdgeSize, Color);

        hintRoot = new GameObject("RollsScrollHint");
        arrows = new[]
        {
            new ScrollArrow { Sign = -1, Renderer = CreateSprite("ArrowLeft", ProceduralSprites.Triangle, 0) },
            new ScrollArrow { Sign = +1, Renderer = CreateSprite("ArrowRight", ProceduralSprites.Triangle, 0) },
        };
        body = CreateSprite("Mouse", ProceduralSprites.MouseBody, 0);
        wheel = CreateSprite("Wheel", ProceduralSprites.MouseWheel, 1);
    }

    void OnDestroy()
    {
        if (hintRoot)
            Destroy(hintRoot);
    }

    void OnEnable() => RollsManager.OnPointerMoved += HandlePointerMoved;
    void OnDisable() => RollsManager.OnPointerMoved -= HandlePointerMoved;

    void HandlePointerMoved(int2 diff)
    {
        if (diff.x == 0)
            return;
        hasScrolled = true;
        foreach (var arrow in arrows)
        {
            if (arrow.Sign == diff.x)
                arrow.Pulse = 1f;
        }
    }

    void Update()
    {
        UpdateEdgeArrows();
        UpdateScrollHint();
    }

    void UpdateEdgeArrows()
    {
        var mouse = UIArrow.RelativeMouse();
        float threshold = CameraController.TransitionThreshold;

        ShowEdge(up, RollsManager.CanMoveUp, UIArrow.Proximity(Vector2.up, mouse, HighlightStart, threshold));
        ShowEdge(down, RollsManager.CanMoveDown, UIArrow.Proximity(Vector2.down, mouse, HighlightStart, threshold));
    }

    void ShowEdge(Image arrow, bool available, float proximity)
    {
        float alpha = available ? Mathf.Lerp(IdleAlpha, 1f, proximity) : 0f;
        UIArrow.Animate(arrow, alpha, Mathf.Lerp(1f, 1.35f, available ? proximity : 0f), FadeSpeed);
    }

    void UpdateScrollHint()
    {
        if (!cam)
            cam = Camera.main;

        bool active = RollsManager.TryGetSelectedRoll(out var rollPos);
        bool justArrived = active && !wasActive;
        wasActive = active;
        if (justArrived)
            hasScrolled = false;

        float dt = Time.unscaledDeltaTime;
        float fade = 1f - Mathf.Exp(-FadeSpeed * dt);
        float decay = Mathf.Exp(-PulseDecay * dt);
        float t = Time.unscaledTime;

        // Before the first scroll: breathe between half and full. After: quiet unless just used.
        float beckon = 0.5f + 0.5f * Mathf.Sin(t * 4f);

        bool canLeft = active && RollsManager.CanScrollLeft;
        bool canRight = active && RollsManager.CanScrollRight;

        float maxAlpha = 0f;
        foreach (var arrow in arrows)
        {
            bool can = arrow.Sign > 0 ? canRight : canLeft;
            arrow.Pulse *= decay;
            float targetAlpha = !can ? 0f
                : hasScrolled ? Mathf.Lerp(IdleAlpha, 1f, arrow.Pulse)
                : Mathf.Lerp(0.45f, 1f, beckon);
            arrow.Alpha = Mathf.Lerp(arrow.Alpha, targetAlpha, fade);
            arrow.Renderer.color = WithAlpha(arrow.Alpha);
            maxAlpha = Mathf.Max(maxAlpha, arrow.Alpha);
        }

        iconAlpha = maxAlpha;
        body.color = WithAlpha(iconAlpha);

        if (!active)
        {
            wheel.color = WithAlpha(0f);
            return;
        }

        var axis = RollsManager.ScrollAxis;
        var closetUp = RollsManager.ClosetUp;
        var normal = RollsManager.ClosetNormal;
        if (cam && Vector3.Dot(normal, cam.transform.position - rollPos) < 0f)
            normal = -normal; // the side of the closet the camera is on

        // Left of the roll: outer arrow at SideDistance, mouse one gap in, right arrow one more gap in.
        var targetPos = rollPos - axis * (SideDistance - IconGap) + normal * FrontOffset;
        position = justArrived ? targetPos : Vector3.Lerp(position, targetPos, 1f - Mathf.Exp(-FollowSpeed * dt));

        // Sprites face the camera side of the closet; each arrow's up points along the closet.
        foreach (var arrow in arrows)
        {
            var dir = axis * arrow.Sign;
            var tr = arrow.Renderer.transform;
            tr.SetPositionAndRotation(position + dir * IconGap, Quaternion.LookRotation(-normal, dir));
            tr.localScale = Vector3.one * ArrowSize * (1f + 0.3f * arrow.Pulse);
        }

        var iconRotation = Quaternion.LookRotation(-normal, closetUp);
        body.transform.SetPositionAndRotation(position, iconRotation);
        body.transform.localScale = Vector3.one * IconSize;

        // Wheel: slides up (scroll right), down (scroll left), or back and forth when both are possible.
        float offset, wheelFade;
        if (canLeft && canRight)
        {
            offset = Mathf.Sin(t * WheelSpeed * Mathf.PI) * 0.07f;
            wheelFade = 1f;
        }
        else
        {
            float phase = Mathf.Repeat(t * WheelSpeed, 1f);
            offset = (phase - 0.5f) * 0.14f * (canRight ? 1f : -1f);
            wheelFade = Mathf.Sin(phase * Mathf.PI);
        }

        wheel.transform.SetPositionAndRotation(position + closetUp * ((ProceduralSprites.WheelY + offset) * IconSize), iconRotation);
        wheel.transform.localScale = Vector3.one * IconSize;
        wheel.color = WithAlpha(iconAlpha * wheelFade);
    }

    Color WithAlpha(float alpha) => new Color(Color.r, Color.g, Color.b, alpha);

    SpriteRenderer CreateSprite(string name, Sprite sprite, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(hintRoot.transform, false);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order; // wheel draws over the mouse body
        renderer.color = WithAlpha(0f);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return renderer;
    }
}
