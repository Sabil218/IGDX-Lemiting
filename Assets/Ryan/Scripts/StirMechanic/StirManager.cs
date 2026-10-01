using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StirManager : MonoBehaviour, ICookingPhase
{
    [Header("Input Hook")]
    [SerializeField] private StirInput stirInput;

    [Header("Scene")]
    [SerializeField] private Transform spoonTransform;

    [Header("Shuffle Settings")]
    public float panBoundaryMultiplier = 0.75f;
    [SerializeField] private Collider2D stirBoundsCollider;
    private Dictionary<Transform, Vector3> ingredientTargets = new Dictionary<Transform, Vector3>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private bool initializedPositions = false;

    [Header("Visuals to Toggle")]
    [SerializeField] private float ingredientSpeedMultiplier = 0.5f;
    [SerializeField] private GameObject[] stirVisuals;

    [Header("Phase Specific Visuals")]
    [SerializeField] private LayeredCookingStage[] phaseCookingStages;

    [Header("Animation")]
    [SerializeField] private Animator spoonAnimator;
    private float stirTimeout = 0.15f;
    private float lastStirTime = -1f;

    [Header("Progress")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private RectTransform progressIcon;
    [SerializeField] private RectTransform progressStartPoint;
    [SerializeField] private RectTransform progressEndPoint;
    [SerializeField] private float completionDelay = 1.0f;

    [Header("Camera Transition")]
    public GameObject stirVirtualCamera;

    private Vector3 GetTruePanCenter()
    {
        if (stirBoundsCollider != null)
        {
            return stirBoundsCollider.bounds.center;
        }
        if (CookingVisualController.Instance != null && CookingVisualController.Instance.foodContentRenderer != null)
        {
            return CookingVisualController.Instance.foodContentRenderer.bounds.center;
        }
        else if (CookingVisualController.Instance != null)
        {
            return CookingVisualController.Instance.transform.position;
        }
        return transform.position;
    }

    private Vector2 GetTruePanExtents()
    {
        if (stirBoundsCollider != null)
        {
            return stirBoundsCollider.bounds.extents;
        }
        if (CookingVisualController.Instance != null && CookingVisualController.Instance.foodContentRenderer != null)
        {
            return CookingVisualController.Instance.foodContentRenderer.bounds.extents;
        }
        if (stirInput != null && stirInput.StirZoneObject != null)
        {
            Collider2D col = stirInput.StirZoneObject.GetComponent<Collider2D>();
            if (col != null) return col.bounds.extents;
        }
        return new Vector2(3f, 1f);
    }

    private void Start()
    {
    }

    // Starts the stirring minigame phase and resets all visuals/positions.
    public void StartPhase()
    {
        if (stirVirtualCamera != null)
        {
            stirVirtualCamera.SetActive(true);
        }

        initializedPositions = false;
        originalPositions.Clear();
        ingredientTargets.Clear();
        SetVisualsActive(true);
    }

    public void SetVisualsActive(bool isActive)
    {
        if (spoonTransform != null) spoonTransform.gameObject.SetActive(isActive);

        if (stirVisuals != null)
        {
            foreach (GameObject visual in stirVisuals)
            {
                if (visual != null) visual.SetActive(isActive);
            }
        }

        if (stirInput != null && stirInput.StirZoneObject != null && !stirInput.StirZoneObject.CompareTag("Wajan"))
        {
            stirInput.StirZoneObject.SetActive(isActive);
        }

        if (isActive && CookingVisualController.Instance != null && phaseCookingStages != null && phaseCookingStages.Length > 0)
        {
            CookingVisualController.Instance.SetLayeredTransitionStages(phaseCookingStages);
        }
    }

    // Toggles the stirring spoon animation based on recent player input.
    private void Update()
    {
        if (spoonAnimator != null)
        {
            bool isActivelyStirring = Time.time - lastStirTime < stirTimeout;
            spoonAnimator.SetBool("isStirring", isActivelyStirring);
        }
    }

    private void OnEnable()
    {
        if (stirInput != null)
        {
            stirInput.OnStirred += HandleStirred;
            stirInput.OnStirProgress += HandleStirProgress;
            stirInput.OnStirCompleted += HandleStirCompleted;
        }
        
        if (progressBarFill != null) progressBarFill.fillAmount = 0f;
        if (progressIcon != null && progressStartPoint != null) progressIcon.position = progressStartPoint.position;
    }

    private void OnDisable()
    {
        if (stirInput != null)
        {
            stirInput.OnStirred -= HandleStirred;
            stirInput.OnStirProgress -= HandleStirProgress;
            stirInput.OnStirCompleted -= HandleStirCompleted;
        }
    }

    // Triggers the visual shuffling of ingredients inside the pan when the player stirs.
    private void HandleStirred(float angleDelta)
    {
        Vector3 panCenter = GetTruePanCenter();
        Vector2 extents = GetTruePanExtents();

        lastStirTime = Time.time;

        if (CookingVisualController.Instance != null && CookingVisualController.Instance.rawIngredientsContainer != null)
        {
            Transform rawContainer = CookingVisualController.Instance.rawIngredientsContainer;

            if (!initializedPositions)
            {
                InitializePositions(rawContainer);
            }

            float moveAmount = Mathf.Abs(angleDelta) * ingredientSpeedMultiplier * 0.005f;

            foreach (Transform rootItem in rawContainer)
            {
                if (rootItem.childCount > 0)
                {
                    foreach (Transform piece in rootItem)
                    {
                        ProcessShuffle(piece, moveAmount);
                    }
                }
                else
                {
                    ProcessShuffle(rootItem, moveAmount);
                }
            }
        }
    }

    private void InitializePositions(Transform rawContainer)
    {
        foreach (Transform rootItem in rawContainer)
        {
            if (rootItem.childCount > 0)
            {
                foreach (Transform piece in rootItem)
                {
                    originalPositions.Add(ClampToPanBounds(piece.position));
                }
            }
            else
            {
                originalPositions.Add(ClampToPanBounds(rootItem.position));
            }
        }
        initializedPositions = true;
    }

    // Moves a specific ingredient piece towards a random target point in the pan to simulate being stirred.
    private void ProcessShuffle(Transform piece, float moveAmount)
    {
        if (!ingredientTargets.ContainsKey(piece))
        {
            AssignNewTarget(piece);
        }

        Vector3 targetPos = ingredientTargets[piece];

        piece.position = Vector3.MoveTowards(piece.position, targetPos, moveAmount);

        if (Vector3.Distance(piece.position, targetPos) < 0.1f)
        {
            AssignNewTarget(piece);
        }
    }

    private void AssignNewTarget(Transform piece)
    {
        ingredientTargets[piece] = GetRandomPointInBounds(piece.position.z);
    }

    private Vector3 GetRandomPointInBounds(float z)
    {
        if (stirBoundsCollider != null)
        {
            Bounds bounds = stirBoundsCollider.bounds;
            for (int attempt = 0; attempt < 30; attempt++)
            {
                Vector2 point = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );
                if (stirBoundsCollider.OverlapPoint(point))
                {
                    return new Vector3(point.x, point.y, z);
                }
            }
            return new Vector3(bounds.center.x, bounds.center.y, z);
        }

        Vector3 center = GetTruePanCenter();
        Vector2 extents = GetTruePanExtents() * panBoundaryMultiplier;
        float a = Mathf.Max(extents.x, 0.1f);
        float b = Mathf.Max(extents.y, 0.1f);
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(Random.Range(0f, 1f));
        return new Vector3(center.x + Mathf.Cos(angle) * radius * a, center.y + Mathf.Sin(angle) * radius * b, z);
    }

    // Ensures ingredients don't accidentally fly out of the pan boundaries.
    private Vector3 ClampToPanBounds(Vector3 worldPos)
    {
        if (stirBoundsCollider != null)
        {
            Vector2 point2D = new Vector2(worldPos.x, worldPos.y);
            if (stirBoundsCollider.OverlapPoint(point2D))
                return worldPos;
            Vector2 closest = stirBoundsCollider.ClosestPoint(point2D);
            return new Vector3(closest.x, closest.y, worldPos.z);
        }

        Vector3 center = GetTruePanCenter();
        Vector2 extents = GetTruePanExtents() * panBoundaryMultiplier;
        float dx = worldPos.x - center.x;
        float dy = worldPos.y - center.y;
        float aVal = Mathf.Max(extents.x, 0.01f);
        float bVal = Mathf.Max(extents.y, 0.01f);
        float ellipseTest = (dx * dx) / (aVal * aVal) + (dy * dy) / (bVal * bVal);
        if (ellipseTest > 1f)
        {
            float scale = 1f / Mathf.Sqrt(ellipseTest);
            dx *= scale;
            dy *= scale;
        }
        return new Vector3(center.x + dx, center.y + dy, worldPos.z);
    }

    // Updates the UI progress bar and visually fades between raw/cooked states.
    private void HandleStirProgress(float progress)
    {
        if (progressBarFill != null) progressBarFill.fillAmount = progress;
        
        if (progressIcon != null && progressStartPoint != null && progressEndPoint != null)
        {
            progressIcon.position = Vector3.Lerp(progressStartPoint.position, progressEndPoint.position, progress);
        }

        if (CookingVisualController.Instance != null)
            CookingVisualController.Instance.UpdateStirProgress(progress);
    }

    // Called when the stirring circle is fully complete.
    private void HandleStirCompleted()
    {
        if (progressBarFill != null) progressBarFill.fillAmount = 1f;
        
        if (progressIcon != null && progressEndPoint != null)
        {
            progressIcon.position = progressEndPoint.position;
        }

        StartCoroutine(DelayedTransition());
    }

    private IEnumerator DelayedTransition()
    {
        yield return new WaitForSeconds(completionDelay);
        SetVisualsActive(false);
        if (CookingManager.instance != null) CookingManager.instance.NextStep();
    }
}