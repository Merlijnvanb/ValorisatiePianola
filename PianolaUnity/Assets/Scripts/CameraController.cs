using UnityEngine;
using Unity.Mathematics;
using System.Collections;
using PrimeTween;

public class CameraController : MonoBehaviour
{
    public float TweenDuration = .75f;
    public CameraPoint[] Points;

    private CameraPoint currentPoint;
    private Sequence currentTweens;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        
        currentPoint = Points[0];
        mainCam.transform.SetPositionAndRotation(Points[0].transform.position, Points[0].transform.rotation);
    }

    void Update()
    {
        if (!mainCam)
            return;
    }

    private bool ChangePoint(CameraPoint point)
    {
        if (currentTweens.isAlive)
            return false;
        
        var startPos = mainCam.transform.position;
        var endPos = point.transform.position;
        
        var startRot = mainCam.transform.rotation;
        var endRot = Quaternion.LookRotation(point.GetViewVector());

        currentTweens = Sequence.Create()
            .Group(Tween.Position(mainCam.transform, startPos, endPos, TweenDuration, Ease.InOutCubic))
            .Group(Tween.Rotation(mainCam.transform, startRot, endRot, TweenDuration, Ease.InOutCubic));
        
        currentPoint = point;

        return true;
    }
    
    // private IEnumerator LerpTransform(Transform oldT, Transform newT)
    // {
    //     var t = 0f;
    //
    //     var oldPos = oldT.transform.position;
    //     var oldRot = oldT.transform.rotation;
    //
    //     var newPos = newT.transform.position;
    //     var newRot = newT.transform.rotation;
    //
    //     while (t < 1f)
    //     {
    //         mainCam.transform.position = Vector3.Lerp(oldPos, newPos, t);
    //         mainCam.transform.rotation = Quaternion.Lerp(oldRot, newRot, t);
    //
    //         t += LerpSpeed * Time.deltaTime;
    //         yield return null;
    //     }
    //     
    //     mainCam.transform.position = newPos;
    //     mainCam.transform.rotation = newRot;
    // }
}
