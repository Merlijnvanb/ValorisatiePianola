using UnityEngine;
using Unity.Mathematics;

public class CameraPoint : MonoBehaviour
{
    [System.Serializable]
    public struct Neighbor
    {
        public CameraPoint point;
        public int2 vector;
    }
    
    public Transform Target;
    public Neighbor[] Neighbors;

    public Vector3 GetViewVector()
    {
        return Target.position - transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, Target.position);
        Gizmos.DrawSphere(Target.position, 0.5f);
    }
}
