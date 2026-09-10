using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StirManager : MonoBehaviour, ICookingPhase
{
    [Header("Input Hook")]
    [SerializeField] private StirInput stirInput;

    [Header("Scene")]
    [Tooltip("Spoon Object")]
    [SerializeField] private Transform spoonTransform;

    [Header("Orbit Settings")]
    [Tooltip("Seberapa dekat jarak orbit sendok ke pinggiran panci")]
    public float orbitMultiplier = 0.75f;
    private Vector2 dynamicOrbitRadii;

    [Header("Visuals to Toggle")]
    [SerializeField] private float ingredientSpeedMultiplier = 0.5f;
    [SerializeField] private GameObject[] stirVisuals;

    [Header("Phase Specific Visuals")]
    [SerializeField] private Sprite[] phaseCookingStages;

    [Header("Progress")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private float completionDelay = 1.0f;

    private float currentOrbitAngle = 0f;

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
        if (spoonTransform != null)
        {
            Vector3 center = GetTruePanCenter();
            Vector3 offset = spoonTransform.position - center;

            CalculateDynamicOrbit();

            float normalizedX = dynamicOrbitRadii.x != 0f ? offset.x / dynamicOrbitRadii.x : 0f;
            float normalizedY = dynamicOrbitRadii.y != 0f ? offset.y / dynamicOrbitRadii.y : 0f;
            currentOrbitAngle = Mathf.Atan2(normalizedY, normalizedX) * Mathf.Rad2Deg;
        }
    }

    private void CalculateDynamicOrbit()
    {
        Vector2 extents = GetTruePanExtents();
        dynamicOrbitRadii = new Vector2(extents.x * orbitMultiplier, extents.y * orbitMultiplier);
    }

    public void StartPhase()
    {
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
            CookingVisualController.Instance.SetTransitionSprites(phaseCookingStages);
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
        
        // Hitung rasio oval sejati wajan
        float squash = extents.x != 0 ? extents.y / extents.x : 1f;

        if (spoonTransform != null)
        {
            currentOrbitAngle += angleDelta;
            float rad = currentOrbitAngle * Mathf.Deg2Rad;

            CalculateDynamicOrbit();

            // Gerak Elips Sendok
            float newX = Mathf.Cos(rad) * dynamicOrbitRadii.x;
            float newY = Mathf.Sin(rad) * dynamicOrbitRadii.y;
            spoonTransform.position = panCenter + new Vector3(newX, newY, 0);

            // Rotasi Dinamis Sendok
            float spoonOrientAngle = currentOrbitAngle - 90f; 
            spoonTransform.rotation = Quaternion.Euler(0, 0, spoonOrientAngle);
        }

        if (CookingVisualController.Instance != null && CookingVisualController.Instance.rawIngredientsContainer != null)
        {
            Transform rawContainer = CookingVisualController.Instance.rawIngredientsContainer;

            float angleRad = angleDelta * ingredientSpeedMultiplier * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            foreach (Transform rootItem in rawContainer)
            {
                if (rootItem.childCount > 0)
                {
                    foreach (Transform piece in rootItem)
                    {
                        ApplyOvalOrbit(piece, panCenter, cos, sin, squash, angleDelta * ingredientSpeedMultiplier);
                    }
                }
                else
                {
                    ApplyOvalOrbit(rootItem, panCenter, cos, sin, squash, angleDelta * ingredientSpeedMultiplier);
                }
            }
        }
    }

    private void ApplyOvalOrbit(Transform target, Vector3 panCenter, float cos, float sin, float squash, float spinAngle)
    {
        Vector3 localPos = target.position - panCenter;
        
        // Cegah error matematika (NaN) jika objek kebetulan berada persis di titik 0,0,0
        if (localPos.sqrMagnitude < 0.001f) 
        {
            target.Rotate(Vector3.forward, spinAngle);
            return;
        }

        float unSquashedY = squash != 0 ? localPos.y / squash : localPos.y;
        
        float rotX = localPos.x * cos - unSquashedY * sin;
        float rotY = localPos.x * sin + unSquashedY * cos;
        float newY = rotY * squash;
        
        target.position = panCenter + new Vector3(rotX, newY, -0.1f);
        target.Rotate(Vector3.forward, spinAngle); 
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
        if (CookingManager.instance != null) CookingManager.instance.NextStep();
    }
}