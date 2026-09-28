using UnityEngine;
using Unity.Mathematics;

public class RollsManager : MonoBehaviour
{
    public CameraController CameraController;
    public CameraPoint RollsPoint;
    public GameObject RollPrefab;
    public int Rows;
    public int Columns;
    public float Width;
    public float Height;
    public float NavigateDuration;

    private Roll[,] rolls;
    private int2 pointer;
    private bool isActive;

    private float spacingWidth;
    private float spacingHeight;

    void OnEnable()
    {
        CameraController.OnPointChanged += HandlePointChanged;
    }

    void OnDisable()
    {
        CameraController.OnPointChanged -= HandlePointChanged;
    }

    void Start()
    {
        spacingWidth = Width / Rows;
        spacingHeight = Height / Columns;
        
        rolls = new Roll[Rows, Columns];

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                var instance = GameObject.Instantiate(RollPrefab, transform);
                rolls[i, j] = instance.GetComponent<Roll>();
                instance.transform.localPosition = new Vector3((i + 0.5f) * spacingWidth - Width/2f, Height/2f - (j + 0.5f) * spacingHeight, 0);
                instance.transform.localEulerAngles = new Vector3(0, 90, 0);
            }
        }
    }

    void Update()
    {
        if (!isActive)
            return;
        
        var rawMousePos = Input.mousePosition;
        var relativeMousePos = new Vector2((rawMousePos.x / Screen.width - .5f) * 2f, (rawMousePos.y / Screen.height - .5f) * 2f);
        var threshold = CameraController.TransitionThreshold;
        
        var transition = new int2();
        
        if (relativeMousePos.x > threshold)
            transition.x = 1;
        else if (relativeMousePos.x < -threshold)
            transition.x = -1;
        
        if (relativeMousePos.y > threshold)
            transition.y = 1;
        else if (relativeMousePos.y < -threshold)
            transition.y = -1;
        
        if (transition.Equals(new int2(0, 1)) && pointer.y > 0)
        {
            if (CameraController.MoveCam(new Vector3(0, spacingHeight, 0), NavigateDuration))
            {
                pointer.y--;
            }
        }
        else if (transition.Equals(new int2(0, -1)) && pointer.y < Columns - 1)
        {
            if (CameraController.MoveCam(new Vector3(0, -spacingHeight, 0), NavigateDuration))
            {
                pointer.y++;
            }
        }
        else if (Input.mouseScrollDelta.y < 0 && pointer.x > 0)
        {
            if (CameraController.MoveCam(new Vector3(0, 0, -spacingWidth), NavigateDuration))
            {
                pointer.x--;
            }
        }
        else if (Input.mouseScrollDelta.y > 0 && pointer.x < Rows - 1)
        {
            if (CameraController.MoveCam(new Vector3(0, 0, spacingWidth), NavigateDuration))
            {
                pointer.x++;
            }
        }
        
        Debug.Log(pointer);
    }

    private void HandlePointChanged(CameraPoint point)
    {
        if (point == RollsPoint)
        {
            isActive = true;
            pointer = int2.zero;
        }
        else
        {
            isActive = false;
        }
    }
}
