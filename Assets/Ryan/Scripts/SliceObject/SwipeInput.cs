using System;
using System.Collections.Generic;
using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public List<Vector2> PathPoints { get; private set; } = new List<Vector2>();

    public event Action<List<Vector2>> OnSwipeCompleted;
    public event Action<Vector2> OnSwipeMoving;

    [SerializeField] private float minimumDistance = 0.2f;

    private bool isDragging;
    private Camera mainCamera;

    //Initialize Camera
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    //Handle Input State
    private void Update()
    {
        // Don't accept any input if the game is paused (Time.timeScale == 0)
        if (Time.timeScale <= 0.0001f)
        {
            if (isDragging)
            {
                isDragging = false;
                PathPoints.Clear();
            }
            return;
        }

        if (IsPointerDown())
        {
            // Handle swipe start (don't start swipe over actual UI menus like pause button)
            if (IsPointerOverUI())
                return;

            isDragging = true;
            PathPoints.Clear();

            Vector2 startPos = GetPointerWorldPos();
            PathPoints.Add(startPos);

            OnSwipeMoving?.Invoke(startPos);
        }
        // Checking while dragging (allow swipe to continue smoothly)
        else if (isDragging && IsPointerHeld())
        {
            Vector2 currentPos = GetPointerWorldPos();
            Vector2 lastPos = PathPoints[PathPoints.Count - 1];

            if (Vector2.Distance(lastPos, currentPos) >= minimumDistance)
            {
                PathPoints.Add(currentPos);
                OnSwipeMoving?.Invoke(currentPos);
            }
        }
        // Lift off
        else if (isDragging && IsPointerUp())
        {
            isDragging = false;
            CompleteSwipe();
        }
    }

    private bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;

        // Use RaycastAll so we only block actual UI menus (PauseButton, settings), NOT gameplay elements like slice guidelines
        var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        pointerEventData.position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == null) continue;

            // Ignore slice guidelines / slice lines
            string name = result.gameObject.name;
            if (name.Contains("garis") || name.Contains("GuideLine") || name.Contains("SliceLine"))
                continue;

            if (result.gameObject.GetComponent<FoodSlicer>() != null)
                continue;

            // It's a real UI element (Pause button, pause popup menu, etc.)
            return true;
        }

        return false;
    }

    //Trigger completion event
    private void CompleteSwipe()
    {
        OnSwipeCompleted?.Invoke(PathPoints);
    }

    // Input Wrapper
    private bool IsPointerDown() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);
    private bool IsPointerHeld() => Input.touchCount > 0 ? (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary) : Input.GetMouseButton(0);
    private bool IsPointerUp() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(0);

    //Convert Screen to World Position
    private Vector2 GetPointerWorldPos()
    {
        Vector2 screenPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z)));
        worldPos.z = 0f; // 2D Space Flatten
        return worldPos;
    }
}