using System;
using UnityEngine;

// Placeholder icons generated in code, so the prototypes need no art. All are white (tint them),
// anti-aliased, and use pixelsPerUnit = texture height so a SpriteRenderer at scale 1 is 1 unit tall.
public static class ProceduralSprites
{
    static Sprite triangle, mouseBody, mouseWheel;

    // Triangle pointing up (+Y).
    public static Sprite Triangle => triangle ? triangle : triangle = Rasterize(64, 64, 64, p =>
    {
        // Apex at the top, base at the bottom, 80% of the height.
        float height = 51f, bottom = -25.5f;
        float py = p.y - bottom;
        float halfWidth = (1f - py / height) * 29f;
        return -Mathf.Min(halfWidth - Mathf.Abs(p.x), py, height - py);
    });

    // Outline of a mouse, upright. The wheel is a separate sprite so it can move.
    public static Sprite MouseBody => mouseBody ? mouseBody : mouseBody = Rasterize(64, 96, 96, p =>
        Mathf.Abs(RoundedBox(p, new Vector2(26f, 42f), 24f)) - 2.5f);

    // Small pill for the wheel; same pixels-per-unit as MouseBody, so they share a scale.
    public static Sprite MouseWheel => mouseWheel ? mouseWheel : mouseWheel = Rasterize(16, 32, 96, p =>
        RoundedBox(p, new Vector2(4.5f, 11f), 4.5f));

    // Wheel center in MouseBody's local space (units, body is 1 tall).
    public const float WheelY = 0.2f;

    // sdf: signed distance in pixels from the shape edge (negative inside), p relative to the texture center.
    static Sprite Rasterize(int width, int height, float pixelsPerUnit, Func<Vector2, float> sdf)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var p = new Vector2(x + 0.5f - width / 2f, y + 0.5f - height / 2f);
                byte alpha = (byte)(Mathf.Clamp01(0.5f - sdf(p)) * 255);
                pixels[y * width + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    static float RoundedBox(Vector2 p, Vector2 halfSize, float radius)
    {
        var q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - halfSize + Vector2.one * radius;
        return Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
    }
}
