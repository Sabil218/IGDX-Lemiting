using System;
using UnityEngine;

public class StirInput : MonoBehaviour
{
    public event Action<float> OnStirred;
    public event Action<float> OnStirProgress;
    public event Action OnStirCompleted;

    [Header("Stir Settings")]
    [SerializeField] private float requiredRotations = 3f;
    private bool requireClockwise = true;
    [SerializeField] private float minimumMoveDelta = 0.2f;

    [Header("Stir Zone & Spoon (World Space)")]
    [SerializeField] private Collider2D stirZoneCollider;

    [Tooltip("Masukkan komponen Collider2D dari sendok ke sini agar drag HANYA dimulai dari sendok.")]
    [SerializeField] private Collider2D spoonCollider;

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
            // Cegah adukan dimulai jika kursor berada di atas elemen UI (Canvas)
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            //Sentuhan pertama mengenai area sendok
            if (spoonCollider != null && spoonCollider.OverlapPoint(worldPos))
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

            float progressDelta = requireClockwise ? -angleDelta : angleDelta;

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
}