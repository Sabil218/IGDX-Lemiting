using System;
using UnityEngine;

public class StirInput : MonoBehaviour
{
    public event Action<float> OnStirred;
    public event Action<float> OnStirProgress;
    public event Action OnStirCompleted;

    [Header("Stir Settings")]
    [SerializeField] private float requiredRotations = 3f;
    [SerializeField] private bool requireClockwise = false;
    [SerializeField] private float minimumMoveDelta = 0.2f;

    [Header("Stir Zone (World Space)")]
    [SerializeField] private Collider2D stirZoneCollider;

    public GameObject StirZoneObject => stirZoneCollider != null ? stirZoneCollider.gameObject : null;
    public Vector2 LastPushDirection { get; private set; } = Vector2.right;

    private bool isDragging;
    private Vector2 previousDirection;
    private float rawCumulativeAngle;
    private float maxCumulativeAngle;
    private float targetAngle;
    private bool isCompleted;
    private Camera mainCamera;

    public float Progress => Mathf.Clamp01(maxCumulativeAngle / targetAngle);

    private void Awake()
    {
        mainCamera = Camera.main;
        EnsureCollider();
    }

    // Resets stirring progress every time the phase starts.
    private void OnEnable()
    {
        EnsureCollider();
        rawCumulativeAngle = 0f;
        maxCumulativeAngle = 0f;
        targetAngle = requiredRotations * 360f;
        isCompleted = false;
        isDragging = false;
    }

    private void EnsureCollider()
    {
        if (stirZoneCollider == null)
        {
            // 1. Cari objek ber-tag Wajan
            GameObject wajan = GameObject.FindWithTag("Wajan");
            if (wajan != null)
            {
                stirZoneCollider = wajan.GetComponent<Collider2D>();
            }

            // 2. Jika tag tidak ditemukan, cari objek bernama Cook_pan
            if (stirZoneCollider == null)
            {
                GameObject pan = GameObject.Find("Cook_pan");
                if (pan != null)
                {
                    stirZoneCollider = pan.GetComponent<Collider2D>();
                }
            }
        }
    }

    // Handles touch/mouse input, calculates circular motion around the pan, and fires progress events.
    private void Update()
    {
        if (isCompleted || Time.timeScale <= 0.0001f)
        {
            if (isDragging) isDragging = false;
            return;
        }

        Vector2 screenPos = GetPointerScreenPos();
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        if (IsPointerDown())
        {
            if (IsPointerOverUI())
                return;

            if (stirZoneCollider != null && stirZoneCollider.OverlapPoint(worldPos))
            {
                isDragging = true;
                previousDirection = (worldPos - GetCenterWorldPos()).normalized;
            }
        }
        else if (isDragging && IsPointerHeld())
        {
            if (IsPointerOverUI())
            {
                isDragging = false;
                return;
            }

            Vector2 center = GetCenterWorldPos();

            float moveDelta = Vector2.Distance(worldPos, center);
            if (moveDelta < minimumMoveDelta) return;

            Vector2 currentDirection = (worldPos - center).normalized;

            float angleDelta = Vector2.SignedAngle(previousDirection, currentDirection);

            Vector2 dirDelta = currentDirection - previousDirection;
            if (dirDelta.sqrMagnitude > 0.0001f)
            {
                Vector2 targetDir = dirDelta.normalized;
                LastPushDirection = Vector2.Lerp(LastPushDirection, targetDir, 0.4f).normalized;
            }

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

    // Finds the center of the stirring zone to calculate the circular rotation angle.
    private Vector2 GetCenterWorldPos()
    {
        EnsureCollider();
        if (stirZoneCollider == null) return Vector2.zero;
        return stirZoneCollider.transform.position;
    }

    private bool IsPointerDown() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);
    private bool IsPointerHeld() => Input.touchCount > 0 ? (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary) : Input.GetMouseButton(0);
    private bool IsPointerUp() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(0);
    private Vector2 GetPointerScreenPos() => Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

    private bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        if (Input.touchCount > 0)
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private void OnDrawGizmos()
    {
        if (stirZoneCollider != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
            Bounds bounds = stirZoneCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stirZoneCollider != null)
        {
            Gizmos.color = Color.green;
            Bounds bounds = stirZoneCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            
            float crosshairSize = 0.5f;
            Gizmos.DrawLine(bounds.center - Vector3.left * crosshairSize, bounds.center + Vector3.left * crosshairSize);
            Gizmos.DrawLine(bounds.center - Vector3.up * crosshairSize, bounds.center + Vector3.up * crosshairSize);
        }
    }
}