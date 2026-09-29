using UnityEngine;
using System.Linq;

public class LightManager : MonoBehaviour
{
    [System.Serializable]
    public struct LightData
    {
        public CameraPoint Point;
        public int[] ActiveLightsIndices;
    }
    
    public Light[] Lights;
    public LightData[] Datas;
    public CameraController CameraController;
    
    void OnEnable()
    {
        CameraController.OnPointChanged += HandlePointChanged;
    }

    void OnDisable()
    {
        CameraController.OnPointChanged -= HandlePointChanged;
    }

    void HandlePointChanged(CameraPoint point)
    {
        foreach (var data in Datas)
        {
            if (point.Equals(data.Point))
            {
                SetLights(data.ActiveLightsIndices);
            }
        }
    }

    void SetLights(int[] indices)
    {
        for (int i = 0; i < Lights.Length; i++)
        {
            if (indices.Contains(i))
            {
                Lights[i].enabled = true;
            }
            else
            {
                Lights[i].enabled = false;
            }
        }
    }
}
