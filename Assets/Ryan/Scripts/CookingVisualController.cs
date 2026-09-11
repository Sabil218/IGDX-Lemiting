using UnityEngine;

public class CookingVisualController : MonoBehaviour
{
    public static CookingVisualController Instance { get; private set; }

    [Header("Visual Components")]
    [Tooltip("Wajan utama (SpriteRenderer atau Image)")]
    public SpriteRenderer panRenderer;
    
    [Tooltip("SpriteRenderer untuk makanan dasar (Base) di dalam wajan")]
    public SpriteRenderer foodContentRenderer;

    [Tooltip("SpriteRenderer untuk overlay makanan (Transisi saat diaduk)")]
    public SpriteRenderer overlayRenderer;

    [Tooltip("Container isi makanan mentah yang jatuh dari fase sebelumnya (akan memudar saat diaduk)")]
    public Transform rawIngredientsContainer;

    [Header("Visual States")]
    [Tooltip("Daftar sprite untuk makanan (0=Mentah, 1=Layu, 2=Matang, dll)")]
    public Sprite[] cookingStages;

    public void SetTransitionSprites(Sprite[] newStages)
    {
        cookingStages = newStages;
        
        // Reset Alpha agar kuah dasar tidak langsung nge-pop/muncul seketika di awal fase mengaduk
        if (foodContentRenderer != null)
        {
            Color c = foodContentRenderer.color;
            c.a = 0f;
            foodContentRenderer.color = c;
        }
        if (overlayRenderer != null)
        {
            Color c = overlayRenderer.color;
            c.a = 0f;
            overlayRenderer.color = c;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Set sprite langsung tanpa transisi (Biasanya dipanggil dari luar saat mulai fase baru)
    public void SetFoodVisual(Sprite newFoodSprite)
    {
        if (foodContentRenderer != null && newFoodSprite != null)
        {
            foodContentRenderer.sprite = newFoodSprite;
            
            Color c = foodContentRenderer.color;
            c.a = 1f;
            foodContentRenderer.color = c;

            // Sembunyikan overlay
            if (overlayRenderer != null)
            {
                Color overlayC = overlayRenderer.color;
                overlayC.a = 0f;
                overlayRenderer.color = overlayC;
            }
        }
    }

    public void SetCookingStage(int stageIndex)
    {
        if (cookingStages != null && stageIndex >= 0 && stageIndex < cookingStages.Length)
        {
            SetFoodVisual(cookingStages[stageIndex]);
        }
    }

    // --- LOGIKA PROGRESS ADUKAN (Dipindahkan dari StirManager) ---
    public void UpdateStirProgress(float progress)
    {
        // 1. Pudarkan bahan mentah yang jatuh
        if (rawIngredientsContainer != null)
        {
            foreach (SpriteRenderer sr in rawIngredientsContainer.GetComponentsInChildren<SpriteRenderer>())
            {
                Color colorA = sr.color; 
                colorA.a = 1f - progress; 
                sr.color = colorA;
            }
        }

        // 2. Transisi Sprite Makanan di wajan (Base ke Overlay)
        if (foodContentRenderer != null && overlayRenderer != null && cookingStages != null && cookingStages.Length > 0)
        {
            int stageCount = cookingStages.Length;
            if (stageCount > 1)
            {
                // Hitung index fase saat ini berdasarkan progress
                float scaledProgress = progress * (stageCount - 1);
                int stageIndex = Mathf.Clamp(Mathf.FloorToInt(scaledProgress), 0, stageCount - 1);
                int nextStageIndex = Mathf.Clamp(stageIndex + 1, 0, stageCount - 1);
                
                // Hitung sisa pecahan untuk alpha transisi (0.0 ke 1.0 di antara dua stage)
                float localStageProgress = scaledProgress - stageIndex;

                foodContentRenderer.sprite = cookingStages[stageIndex];
                overlayRenderer.sprite = cookingStages[nextStageIndex];

                // Base memudar masuk di awal (0% -> 100% pada 25% progress pertama), Overlay memudar masuk perlahan
                Color colorBase = foodContentRenderer.color; 
                colorBase.a = Mathf.Clamp01(progress * 4f); 
                foodContentRenderer.color = colorBase;

                Color colorOverlay = overlayRenderer.color; 
                colorOverlay.a = localStageProgress; 
                overlayRenderer.color = colorOverlay;
            }
            else
            {
                // Jika hanya ada 1 gambar, tidak ada transisi
                foodContentRenderer.sprite = cookingStages[0]; 
                overlayRenderer.sprite = cookingStages[0];
            }
        }
    }
}
