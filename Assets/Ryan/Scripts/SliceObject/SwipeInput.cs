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
        if (IsPointerDown())
        {
            // Cegah swipe dimulai jika kursor berada di atas elemen UI (Canvas)
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            isDragging = true;
            PathPoints.Clear();

            Vector2 startPos = GetPointerWorldPos();
            PathPoints.Add(startPos);

            OnSwipeMoving?.Invoke(startPos);
        }
        //Checking while dragging
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

    //Trigger completion event
    private void CompleteSwipe()
    {
        OnSwipeCompleted?.Invoke(PathPoints);
    }

    // Input Wrapper
    private bool IsPointerDown() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);
    private bool IsPointerHeld() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Moved : Input.GetMouseButton(0);
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