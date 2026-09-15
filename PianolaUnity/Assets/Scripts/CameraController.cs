using UnityEngine;
using Unity.Mathematics;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public struct Neighbor
    {
        public CameraPoint point;
        public int2 vector;
    }
    
    [System.Serializable]
    public struct Point
    {
        public CameraPoint mainPoint;
        public Neighbor[] neighbors;
    }

    public float LerpSpeed = .75f;
    public Point[] Points;

    private Point currentPoint;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        ChangePoint(Points[0]);
    }

    void Update()
    {
        if (!mainCam)
            return;
    }

    private void ChangePoint(Point point)
    {
        StartCoroutine(LerpTransform(mainCam.transform, point.mainPoint.transform));
        currentPoint = point;
    }
    
    private IEnumerator LerpTransform(Transform oldT, Transform newT)
    {
        var t = 0f;

        var oldPos = oldT.transform.position;
        var oldRot = oldT.transform.rotation;

        var newPos = newT.transform.position;
        var newRot = newT.transform.rotation;

        while (t < 1f)
        {
            mainCam.transform.position = Vector3.Slerp(oldPos, newPos, t);
            mainCam.transform.rotation = Quaternion.Slerp(oldRot, newRot, t);

            t += LerpSpeed * Time.deltaTime;
            yield return null;
        }
        
        mainCam.transform.position = newPos;
        mainCam.transform.rotation = newRot;
    }
}
