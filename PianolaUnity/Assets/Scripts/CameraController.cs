using UnityEngine;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public struct Point
    {
        public CameraPoint mainPoint;
        public CameraPoint[] neighbors;
    }

    public Point[] Points;
    
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }
}
