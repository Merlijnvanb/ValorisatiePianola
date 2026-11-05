using UnityEngine;
using UnityEngine.UI;

// Shared helpers for the placeholder navigation arrows (EdgeIndicators, RollsIndicators).
public static class UIArrow
{
    // An arrow anchored at the screen edge/corner in `dir`, `margin` canvas pixels inward, pointing outward.
    public static Image Create(Transform parent, string name, Vector2 dir, float margin, float size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.sprite = ProceduralSprites.Triangle;
        image.raycastTarget = false; // never block clicks
        image.color = new Color(color.r, color.g, color.b, 0f);

        var rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f) + dir * 0.5f;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = -dir * margin;
        rect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);

        return image;
    }

    // Eases an arrow's alpha and scale towards a target; call every frame.
    public static void Animate(Image arrow, float targetAlpha, float targetScale, float fadeSpeed)
    {
        float step = 1f - Mathf.Exp(-fadeSpeed * Time.unscaledDeltaTime);

        var c = arrow.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, step);
        arrow.color = c;

        float scale = Mathf.Lerp(arrow.rectTransform.localScale.x, targetScale, step);
        arrow.rectTransform.localScale = Vector3.one * scale;
    }

    // Mouse position with the screen mapped to -1..1 on both axes, like CameraController uses.
    public static Vector2 RelativeMouse()
    {
        var mouse = Input.mousePosition;
        return new Vector2((mouse.x / Screen.width - .5f) * 2f, (mouse.y / Screen.height - .5f) * 2f);
    }

    // 0 while the mouse is closer to the center than `start`, 1 once it reaches `threshold` in `dir`.
    // For corners both axes have to be close.
    public static float Proximity(Vector2 dir, Vector2 relativeMouse, float start, float threshold)
    {
        float result = 1f;
        if (dir.x != 0)
            result = Mathf.Min(result, Mathf.InverseLerp(start, threshold, relativeMouse.x * dir.x));
        if (dir.y != 0)
            result = Mathf.Min(result, Mathf.InverseLerp(start, threshold, relativeMouse.y * dir.y));
        return result;
    }
}
