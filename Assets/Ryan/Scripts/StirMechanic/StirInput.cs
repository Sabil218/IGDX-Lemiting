using System;
using UnityEngine;

public class StirInput : MonoBehaviour
{
    public event Action<float> OnStirred;
    public event Action<float> OnStirProgress;
    public event Action OnStirCompleted;

    [Header("Stir Settings")]
    [SerializeField] private float requiredRotations = 3f;
    [Tooltip("If false, player can stir in any circular direction (clockwise or counter-clockwise)")]
    [SerializeField] private bool requireClockwise = false;
    [SerializeField] private float minimumMoveDelta = 0.2f;

    [Header("Stir Zone (World Space)")]
    [SerializeField] private Collider2D stirZoneCollider;

    public GameObject StirZoneObject => stirZoneCollider != null ? stirZoneCollider.gameObject : null;

    private bool isDragging;
    private Vector2 previousDirection;
    private float rawCumulativeAngle;
    private float maxCumulativeAngle;
    private float targetAngle;
    private bool isCompleted;
    private Camera mainCamera;

    public float Progress => Mathf.Clamp01(maxCumulativeAngle / targetAngle);

    //Initialize camera
    private void Awake() => mainCamera = Camera.main;

    //Reset input state
    private void OnEnable()
    {
        rawCumulativeAngle = 0f;
        maxCumulativeAngle = 0f;
        targetAngle = requiredRotations * 360f;
        isCompleted = false;
        isDragging = false;
    }

    //Process input dragging logic
    private void Update()
    {
        if (isCompleted) return;

        Vector2 screenPos = GetPointerScreenPos();
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        if (IsPointerDown())
        {
            //Validator Input
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            //Input checking: Start dragging if touching the stir zone (pan)
            if (stirZoneCollider != null && stirZoneCollider.OverlapPoint(worldPos))
            {
                isDragging = true;
                previousDirection = (worldPos - GetCenterWorldPos()).normalized;
            }
        }
        else if (isDragging && IsPointerHeld())
        {
            Vector2 center = GetCenterWorldPos();

            float moveDelta = Vector2.Distance(worldPos, center);
            if (moveDelta < minimumMoveDelta) return;

            Vector2 currentDirection = (worldPos - center).normalized;

            float angleDelta = Vector2.SignedAngle(previousDirection, currentDirection);

            // Allow any direction if requireClockwise is false, otherwise strictly check direction
            float progressDelta = requireClockwise ? -angleDelta : Mathf.Abs(angleDelta);

            if (Mathf.Abs(angleDelta) > 0.5f && Mathf.Abs(angleDelta) < 90f)
            {
                rawCumulativeAngle += progressDelta;
                if (rawCumulativeAngle > maxCumulativeAngle)
                {
                    float newProgressAdded = rawCumulativeAngle - maxCumulativeAngle;
                    maxCumulativeAngle = rawCumulativeAngle;

                    float spoonAngleDelta = requireClockwise ? -newProgressAdded : newProgressAdded;

                    OnStirred?.Invoke(spoonAngleDelta);

                    float progress = Mathf.Clamp01(maxCumulativeAngle / targetAngle);
                    OnStirProgress?.Invoke(progress);

                    if (maxCumulativeAngle >= targetAngle)
                    {
                        isCompleted = true;
                        OnStirCompleted?.Invoke();
                    }
                }
            }
            previousDirection = currentDirection;
        }
        else if (isDragging && IsPointerUp())
        {
            isDragging = false;
        }
    }

    //Get stir zone center
    private Vector2 GetCenterWorldPos()
    {
        if (stirZoneCollider == null) return Vector2.zero;
        return stirZoneCollider.transform.position;
    }

    //Input Wrappers
    private bool IsPointerDown() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);
    private bool IsPointerHeld() => Input.touchCount > 0 ? (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary) : Input.GetMouseButton(0);
    private bool IsPointerUp() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(0);
    private Vector2 GetPointerScreenPos() => Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

    private void OnDrawGizmos()
    {
        if (stirZoneCollider != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.4f); // Semi-transparent green
            Bounds bounds = stirZoneCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stirZoneCollider != null)
        {
            Gizmos.color = Color.green; // Solid green when selected
            Bounds bounds = stirZoneCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            
            // Draw a crosshair in the center
            float crosshairSize = 0.5f;
            Gizmos.DrawLine(bounds.center - Vector3.left * crosshairSize, bounds.center + Vector3.left * crosshairSize);
            Gizmos.DrawLine(bounds.center - Vector3.up * crosshairSize, bounds.center + Vector3.up * crosshairSize);
        }
    }
}