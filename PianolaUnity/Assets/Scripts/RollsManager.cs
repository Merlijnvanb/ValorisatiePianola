using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    // For UI (RollsIndicators): where the closet can be navigated right now.
    public bool IsActive => isActive;
    public bool CanMoveUp => isActive && CanMove(Up);
    public bool CanMoveDown => isActive && CanMove(Down);
    public bool CanScrollLeft => isActive && CanMove(Left);
    public bool CanScrollRight => isActive && CanMove(Right);
    // Along the closet towards the next column, i.e. scrolling "right" (scroll wheel up).
    public Vector3 ScrollAxis => transform.right;
    public Vector3 ClosetUp => transform.up;
    public Vector3 ClosetNormal => transform.forward;

    public bool TryGetSelectedRoll(out Vector3 position)
    {
        var roll = isActive ? rolls[pointer.x, pointer.y] : null;
        position = roll ? roll.transform.position : default;
        return roll;
    }
    public event Action<int2> OnPointerMoved;

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

        // Recordings made earlier (see RollRecorder) go after the start rolls.
        var allRolls = new List<RollData>(StartRolls);
        allRolls.AddRange(RecordedRolls.LoadAll());
        foreach (var data in allRolls)
        {
            if (!AddRoll(data))
                break;
        }
    }

    // Puts a roll in the next free slot of the closet. Returns false when the closet is full.
    public bool AddRoll(RollData data)
    {
        if (rollsAmount >= Rows * Columns)
            return false;

        // Fills a column bottom to top, then moves to the next column.
        int i = rollsAmount / Columns;
        int j = rollsAmount % Columns;

        var instance = Instantiate(RollPrefab, transform);
        var rollComponent = instance.GetComponent<Roll>();
        rolls[i, j] = rollComponent;
        rollComponent.Initialize(data);
        instance.transform.localPosition = new Vector3((i + 0.5f) * spacingWidth - Width / 2f, (j + 0.5f) * spacingHeight - Height / 2f, 0);
        instance.transform.localEulerAngles = new Vector3(0, 90, 0);

        rollsAmount++;
        return true;
    }

    void Update()
    {
        if (!isActive)
            return;
        
        // Clicks on UI buttons (Opnemen etc.) should not also pick a roll.
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
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

    private static bool IsPointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

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
        OnPointerMoved?.Invoke(diff);
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
