using UnityEngine;
using Unity.Mathematics;
using System.Collections;
using PrimeTween;

public class CameraController : MonoBehaviour
{
    public float TweenDuration = .75f;
    public float TransitionThreshold = .95f;
    public CameraPoint StartPoint;

    private CameraPoint currentPoint;
    private Sequence currentTweens;
    private Camera mainCam { get => GetCamera(); }
    private Camera _mainCam;

    void Start()
    {
        currentPoint = StartPoint;
        
        var startPos = StartPoint.transform.position;
        var startRot = Quaternion.LookRotation(StartPoint.GetViewVector());
        mainCam.transform.SetPositionAndRotation(startPos, startRot);
    }

    void Update()
    {
        var rawMousePos = Input.mousePosition;
        var relativeMousePos = new Vector2((rawMousePos.x / Screen.width - .5f) * 2f, (rawMousePos.y / Screen.height - .5f) * 2f);
        Debug.Log(relativeMousePos);

        var transition = new int2();
        
        if (relativeMousePos.x > TransitionThreshold)
            transition.x = 1;
        else if (relativeMousePos.x < -TransitionThreshold)
            transition.x = -1;
        
        if (relativeMousePos.y > TransitionThreshold)
            transition.y = 1;
        else if (relativeMousePos.y < -TransitionThreshold)
            transition.y = -1;

        foreach (var neighbor in currentPoint.Neighbors)
        {
            if (transition.Equals(neighbor.Transition))
                ChangePoint(neighbor.Point);
        }
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
            .Group(Tween.Position(mainCam.transform, startPos, endPos, TweenDuration, Ease.OutExpo))
            .Group(Tween.Rotation(mainCam.transform, startRot, endRot, TweenDuration, Ease.OutExpo));
        
        currentPoint = point;

        return true;
    }

    private Camera GetCamera()
    {
        if (!_mainCam)
            _mainCam = Camera.main;

        return _mainCam;
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
