using System;
using System.Collections.Generic;
using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public List<Vector2> PathPoints { get; private set; } = new();
    public event Action<List<Vector2>> OnSwipeCompleted;

    [SerializeField] private float minSwipeDistance = 0.5f;

    private bool isDragging;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (IsPointerDown())
        {
            isDragging = true;
            PathPoints.Clear();
            PathPoints.Add(GetPointerWorldPos());
        }
        else if (isDragging && IsPointerHeld())
        {
            PathPoints.Add(GetPointerWorldPos());
        }
        else if (isDragging && IsPointerUp())
        {
            isDragging = false;
            CompleteSwipe();
        }
    }

    private void CompleteSwipe()
    {
        if (PathPoints.Count < 2) return; //Memastikan dragging

        float swipeDistance = Vector2.Distance(PathPoints[0], PathPoints[^1]);
        if (swipeDistance >= minSwipeDistance)
            OnSwipeCompleted?.Invoke(PathPoints);
    }

    private bool IsPointerDown() => Input.touchCount > 0
        ? Input.GetTouch(0).phase == TouchPhase.Began
        : Input.GetMouseButtonDown(0);

    private bool IsPointerHeld() => Input.touchCount > 0
        ? Input.GetTouch(0).phase == TouchPhase.Moved
        : Input.GetMouseButton(0);

    private bool IsPointerUp() => Input.touchCount > 0
        ? Input.GetTouch(0).phase == TouchPhase.Ended
        : Input.GetMouseButtonUp(0);

    private Vector2 GetPointerWorldPos()
    {
        Vector2 screenPos = Input.touchCount > 0
            ? Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;
        return mainCamera.ScreenToWorldPoint(screenPos);
    }
}