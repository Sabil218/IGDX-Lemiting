using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StirManager : MonoBehaviour, ICookingPhase
{
    [Header("Input Hook")]
    [SerializeField] private StirInput stirInput;

    [Header("Scene")]
    [Tooltip("Masukkan langsung objek Sendok (bukan parent pivot) ke sini.")]
    [SerializeField] private Transform spoonTransform;

    [Tooltip("Objek placeholder bahan makanan")]
    [SerializeField] private Transform ingredientsTransform;

    [Header("Crossfade Visuals")]
    [Tooltip("Sprite bahan saat masih mentah (awal)")]
    [SerializeField] private SpriteRenderer rawVisual;
    [Tooltip("Sprite bahan saat matang/tercampur (akhir)")]
    [SerializeField] private SpriteRenderer cookedVisual;

    [SerializeField] private float ingredientSpeedMultiplier = 0.5f;

    [Header("Visuals to Toggle")]
    [SerializeField] private GameObject[] stirVisuals;

    [Header("Progress")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private float completionDelay = 1.0f;

    private Quaternion lockedSpoonRotation;

    //Initialize spoon rotation
    private void Start()
    {
        if (spoonTransform != null)
        {
            lockedSpoonRotation = spoonTransform.rotation;
        }
    }

    //Start stirring phase
    public void StartPhase()
    {
        SetVisualsActive(true);
    }

    //Toggle visuals state
    public void SetVisualsActive(bool isActive)
    {
        if (spoonTransform != null) spoonTransform.gameObject.SetActive(isActive);
        if (ingredientsTransform != null) ingredientsTransform.gameObject.SetActive(isActive);

        if (stirVisuals != null)
        {
            foreach (GameObject visual in stirVisuals)
            {
                if (visual != null) visual.SetActive(isActive);
            }
        }

        if (stirInput != null && stirInput.StirZoneObject != null)
        {
            // Pengecekan diganti menggunakan Tag untuk menghindari hardcoded string name
            if (!stirInput.StirZoneObject.CompareTag("Wajan"))
            {
                stirInput.StirZoneObject.SetActive(isActive);
            }
        }

        // Reset opasitas ke kondisi awal saat fase diaktifkan
        if (isActive)
        {
            if (rawVisual != null) { Color c = rawVisual.color; c.a = 1f; rawVisual.color = c; }
            if (cookedVisual != null) { Color c = cookedVisual.color; c.a = 0f; cookedVisual.color = c; }
        }
    }

    //Subscribe to stir events
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

    //Unsubscribe from stir events
    private void OnDisable()
    {
        if (stirInput != null)
        {
            stirInput.OnStirred -= HandleStirred;
            stirInput.OnStirProgress -= HandleStirProgress;
            stirInput.OnStirCompleted -= HandleStirCompleted;
        }
    }

    //Rotate objects and handle logic
    private void HandleStirred(float angleDelta)
    {
        if (stirInput == null || stirInput.StirZoneObject == null) return;
        Vector3 panCenter = stirInput.StirZoneObject.transform.position;

        if (spoonTransform != null)
        {
            // Mengorbitkan posisi sendok
            spoonTransform.RotateAround(panCenter, Vector3.forward, angleDelta);
            
            // Mengunci rotasi agar tetap miring seperti di Editor
            spoonTransform.rotation = lockedSpoonRotation;

            // --- LOGIKA AUTO FLIP (Opsional) ---
            Vector3 skalaSendok = spoonTransform.localScale;
            if (spoonTransform.position.x < panCenter.x)
                skalaSendok.x = -Mathf.Abs(skalaSendok.x); // Balik gambar (Flip X) saat di kiri
            else
                skalaSendok.x = Mathf.Abs(skalaSendok.x);  // Normal saat di kanan
                
            spoonTransform.localScale = skalaSendok;
            // --- BATAS AKHIR LOGIKA AUTO FLIP ------------------------------
        }

        if (ingredientsTransform != null)
        {
            // Mengorbitkan bahan makanan
            ingredientsTransform.RotateAround(panCenter, Vector3.forward, angleDelta * ingredientSpeedMultiplier);
        }
    }

    //Update visuals and UI based on progress
    private void HandleStirProgress(float progress)
    {
        if (progressBarFill != null) progressBarFill.fillAmount = progress;

        // Logika Crossfade Opasitas
        if (rawVisual != null)
        {
            Color rawColor = rawVisual.color;
            rawColor.a = 1f - progress; // Mentah memudar perlahan
            rawVisual.color = rawColor;
        }

        if (cookedVisual != null)
        {
            Color cookedColor = cookedVisual.color;
            cookedColor.a = progress; // Matang muncul perlahan
            cookedVisual.color = cookedColor;
        }
    }

    //Trigger stage completion
    private void HandleStirCompleted()
    {
        Debug.Log("[StirManager] POC Adukan Selesai! Pindah ke fase berikutnya.");
        if (progressBarFill != null) progressBarFill.fillAmount = 1f;
        StartCoroutine(DelayedTransition());
    }

    //Delay before notifying manager
    private IEnumerator DelayedTransition()
    {
        yield return new WaitForSeconds(completionDelay);
        if (CookingManager.instance != null) CookingManager.instance.NextStep();
    }
}