using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StirManager : MonoBehaviour, ICookingPhase
{
    [Header("Input Hook")]
    [SerializeField] private StirInput stirInput;

    [Header("Scene")]
    [Tooltip("Spoon Object")]
    [SerializeField] private Transform spoonTransform;

    [Header("Shuffle Settings")]
    [Tooltip("Seberapa jauh bahan bisa bergerak dari pusat panci saat diaduk")]
    public float panBoundaryMultiplier = 0.75f;
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
    [SerializeField] private float completionDelay = 1.0f;


    // Mengambil PUSAT WAJAN secara visual (akurat dari gambar kuah)
    private Vector3 GetTruePanCenter()
    {
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

    // Mengambil UKURAN WAJAN secara visual (akurat dari gambar kuah)
    private Vector2 GetTruePanExtents()
    {
        if (CookingVisualController.Instance != null && CookingVisualController.Instance.foodContentRenderer != null)
        {
            return CookingVisualController.Instance.foodContentRenderer.bounds.extents;
        }
        if (stirInput != null && stirInput.StirZoneObject != null)
        {
            Collider2D col = stirInput.StirZoneObject.GetComponent<Collider2D>();
            if (col != null) return col.bounds.extents;
        }
        return new Vector2(3f, 1f); // Fallback
    }

    private void Start()
    {
        // Setup logic can go here if needed in the future
    }

    public void StartPhase()
    {
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

    private void HandleStirred(float angleDelta)
    {
        Vector3 panCenter = GetTruePanCenter();
        Vector2 extents = GetTruePanExtents();

        // Catat waktu stir terakhir untuk memicu animasi
        lastStirTime = Time.time;

        if (CookingVisualController.Instance != null && CookingVisualController.Instance.rawIngredientsContainer != null)
        {
            Transform rawContainer = CookingVisualController.Instance.rawIngredientsContainer;

            if (!initializedPositions)
            {
                InitializePositions(rawContainer);
            }

            // Seberapa cepat bahan bergerak bergeser berdasarkan kecepatan putaran adukan (angleDelta)
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
                    originalPositions.Add(piece.position);
                }
            }
            else
            {
                originalPositions.Add(rootItem.position);
            }
        }
        initializedPositions = true;
    }

    private void ProcessShuffle(Transform piece, float moveAmount)
    {
        // Jika piece belum punya target tujuan, berikan target baru secara acak
        if (!ingredientTargets.ContainsKey(piece))
        {
            AssignNewTarget(piece);
        }

        Vector3 targetPos = ingredientTargets[piece];

        // Pindahkan piece secara mulus ke arah targetnya
        piece.position = Vector3.MoveTowards(piece.position, targetPos, moveAmount);

        // Jika piece sudah sangat dekat dengan targetnya, beri target baru lagi agar dia terus bergerak / "shuffle"
        if (Vector3.Distance(piece.position, targetPos) < 0.1f)
        {
            AssignNewTarget(piece);
        }
    }

    private void AssignNewTarget(Transform piece)
    {
        if (originalPositions.Count == 0) return;
        
        // Pilih satu tempat duduk acak dari daftar posisi awal
        int randomIndex = Random.Range(0, originalPositions.Count);
        ingredientTargets[piece] = originalPositions[randomIndex];
    }

    private void HandleStirProgress(float progress)
    {
        if (progressBarFill != null) progressBarFill.fillAmount = progress;

        if (CookingVisualController.Instance != null)
            CookingVisualController.Instance.UpdateStirProgress(progress);
    }

    private void HandleStirCompleted()
    {
        if (progressBarFill != null) progressBarFill.fillAmount = 1f;
        StartCoroutine(DelayedTransition());
    }

    private IEnumerator DelayedTransition()
    {
        yield return new WaitForSeconds(completionDelay);
        SetVisualsActive(false);
        if (CookingManager.instance != null) CookingManager.instance.NextStep();
    }
}