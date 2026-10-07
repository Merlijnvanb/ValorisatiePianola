using UnityEngine;
using Unity.Mathematics;

public class RollsManager : MonoBehaviour
{
    public CameraController CameraController;
    
    public CameraPoint RollsPoint;
    public GameObject RollPrefab;
    public MidiSequencer MidiSequencer;
    public CameraPoint PianolaPoint;
    
    public Transform SpotLightTransform;
    public float LightAheadAmount;
    
    public int Rows;
    public int Columns;
    public float Width;
    public float Height;
    public float NavigateDuration;
    public RollData[] StartRolls;
    
    
    private static readonly int2 Up = new int2(0, 1);
    private static readonly int2 Down = new int2(0, -1);
    private static readonly int2 Left = new int2(-1, 0);
    private static readonly int2 Right = new int2(1, 0);

    private Roll[,] rolls;
    private int2 pointer;
    private bool isActive;
    private Vector3 lightStartPos;

    private float spacingWidth;
    private float spacingHeight;

    private int rollsAmount;

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
        lightStartPos = SpotLightTransform.position;
        spacingWidth = Width / Rows;
        spacingHeight = Height / Columns;
        
        rolls = new Roll[Rows, Columns];
        rollsAmount = StartRolls.Length;
        
        var count = 0;
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (count >= StartRolls.Length)
                    break;
                
                var instance = Instantiate(RollPrefab, transform);
                var rollComponent = instance.GetComponent<Roll>();
                rolls[i, j] = rollComponent;
                rollComponent.Initialize(StartRolls[count]);
                instance.transform.localPosition = new Vector3((i + 0.5f) * spacingWidth - Width / 2f, (j + 0.5f) * spacingHeight - Height / 2f, 0);
                instance.transform.localEulerAngles = new Vector3(0, 90, 0);
                
                count++;
            }
        }
    }

    void Update()
    {
        if (!isActive)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            PickRoll();
            return;
        }
        
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
        
        if (transition.Equals(Up) && CanMove(Up))
        {
            if (TryPanTo(new Vector3(0, spacingHeight, 0)))
            {
                HandlePointerShift(Up);
            }
        }
        else if (transition.Equals(Down) && CanMove(Down))
        {
            if (TryPanTo(new Vector3(0, -spacingHeight, 0)))
            {
                HandlePointerShift(Down);
            }
        }
        else if (Input.mouseScrollDelta.y < 0 && CanMove(Left))
        {
            if (TryPanTo(new Vector3(0, 0, -spacingWidth)))
            {
                HandlePointerShift(Left);
            }
        }
        else if (Input.mouseScrollDelta.y > 0 && CanMove(Right))
        {
            if (TryPanTo(new Vector3(0, 0, spacingWidth)))
            {
                HandlePointerShift(Right);
            }
        }
        
        Debug.Log(pointer);
    }

    private void PickRoll()
    {
        if (!CameraController.ChangePoint(PianolaPoint))
            return;
        
        var roll = rolls[pointer.x, pointer.y];
        MidiSequencer.Load(roll.Data.MidiPath);
        MidiSequencer.Play();
    }

    private bool CanMove(int2 direction)
    {
        if (direction.Equals(Up))
        {
            if (pointer.y >= Columns - 1)
                return false;

            if (pointer.y + 1 >= rollsAmount - pointer.x * Columns)
                return false;
        }
        else if (direction.Equals(Down))
        {
            if (pointer.y <= 0)
                return false;
        }
        else if (direction.Equals(Right))
        {
            if (pointer.x >= Rows - 1)
                return false;

            if (pointer.y >= rollsAmount - (pointer.x + 1) * Columns)
                return false;
        }
        else if (direction.Equals(Left))
        {
            if (pointer.x <= 0)
                return false;
        }

        return true;
    }

    private void HandlePointerShift(int2 diff)
    {
        rolls[pointer.x,pointer.y].Disable();
        pointer += diff;
        rolls[pointer.x,pointer.y].Enable();
    }

    private bool TryPanTo(Vector3 vector)
    {
        if (CameraController.MoveCam(vector, NavigateDuration))
        {
            SpotLightTransform.position += vector;
            return true;
        }
        
        return false;
    }

    private void HandlePointChanged(CameraPoint point)
    {
        if (point == RollsPoint)
        {
            isActive = true;
            pointer = int2.zero;
            rolls[pointer.x,pointer.y].Enable();
            SpotLightTransform.position = lightStartPos;
        }
        else
        {
            if (isActive)
            {
                rolls[pointer.x,pointer.y].Disable();
            }
            
            isActive = false;
        }
    }
}
