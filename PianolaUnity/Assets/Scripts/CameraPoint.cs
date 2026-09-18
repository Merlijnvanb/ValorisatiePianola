using UnityEngine;
using Unity.Mathematics;

[ExecuteInEditMode]
public class CameraPoint : MonoBehaviour
{
    [System.Serializable]
    public struct Neighbor
    {
        public CameraPoint Point;
        public int2 Transition;
    }
    
    public Transform Target;
    public Neighbor[] Neighbors;

    public Vector3 GetViewVector()
    {
        return Target.position - transform.position;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, Target.position);
        Gizmos.DrawSphere(Target.position, 0.1f);
    }
}
