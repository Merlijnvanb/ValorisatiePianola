using System;
using UnityEngine;

// Drives the "Pianola/Cutaway Lit" shader: at the listed camera points it opens a hole in every surface
// using that shader (the pianola shell), along the line from the camera to the point's target.
// Internals use normal materials and are never cut. The hole grows/shrinks and slides smoothly.
public class ObstructionController : MonoBehaviour
{
    [Serializable]
    public struct CutawayPoint
    {
        public CameraPoint Point;
        [Tooltip("Radius of the hole in world units")]
        public float Radius;
    }

    public CameraController CameraController;
    public CutawayPoint[] Points;
    [Tooltip("Width of the dithered edge of the hole, in world units")]
    public float Softness = 0.08f;
    [Tooltip("How fast the hole opens, closes and moves")]
    public float Speed = 6f;

    static readonly int FromId = Shader.PropertyToID("_CutawayFrom");
    static readonly int ToId = Shader.PropertyToID("_CutawayTo");
    static readonly int RadiusId = Shader.PropertyToID("_CutawayRadius");
    static readonly int SoftnessId = Shader.PropertyToID("_CutawaySoftness");

    float radius;
    float targetRadius;
    Vector3 focus;
    Vector3 targetFocus;
    Camera cam;

    void OnEnable() => CameraController.OnPointChanged += HandlePointChanged;

    void OnDisable()
    {
        CameraController.OnPointChanged -= HandlePointChanged;
        Shader.SetGlobalFloat(RadiusId, 0f);
    }

    void Start()
    {
        if (CameraController.CurrentPoint)
            HandlePointChanged(CameraController.CurrentPoint);
    }

    void HandlePointChanged(CameraPoint point)
    {
        targetRadius = 0f;
        foreach (var cutaway in Points)
        {
            if (cutaway.Point != point)
                continue;

            targetRadius = cutaway.Radius;
            targetFocus = point.Target.position;
            // Opening from closed: start at the new spot instead of sliding in from the old one.
            if (radius < 0.01f)
                focus = targetFocus;
        }
    }

    // LateUpdate so the camera has already moved this frame.
    void LateUpdate()
    {
        if (!cam)
            cam = Camera.main;
        if (!cam)
            return;

        float step = 1f - Mathf.Exp(-Speed * Time.unscaledDeltaTime);
        radius = Mathf.Lerp(radius, targetRadius, step);
        focus = Vector3.Lerp(focus, targetFocus, step);

        Shader.SetGlobalVector(FromId, cam.transform.position);
        Shader.SetGlobalVector(ToId, focus);
        Shader.SetGlobalFloat(RadiusId, radius < 0.005f ? 0f : radius);
        Shader.SetGlobalFloat(SoftnessId, Softness);
    }
}
