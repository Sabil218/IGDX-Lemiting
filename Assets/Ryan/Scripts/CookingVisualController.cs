using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct LayeredCookingStage
{
    [Tooltip("GameObject Base/Bawah yang sudah diletakkan di scene (misal: kuah merah). Disable saja di Inspector, script akan mengatur alpha-nya.")]
    public SpriteRenderer baseRenderer;
    
    [Tooltip("GameObject Front/Atas yang sudah diletakkan di scene (misal: ayam matang). Disable saja di Inspector, script akan mengatur alpha-nya.")]
    public SpriteRenderer frontRenderer;
}

public class CookingVisualController : MonoBehaviour
{
    public static CookingVisualController Instance { get; private set; }

    [Header("Visual Components")]
    [Tooltip("Wajan utama (SpriteRenderer atau Image)")]
    public SpriteRenderer panRenderer;
    
    [Tooltip("SpriteRenderer untuk makanan dasar (Base) di dalam wajan (Layer 1)")]
    public SpriteRenderer foodContentRenderer;

    [Tooltip("SpriteRenderer untuk overlay makanan depan (Layer 3)")]
    public SpriteRenderer frontOverlayRenderer;

    [Tooltip("Container isi makanan mentah yang jatuh dari fase sebelumnya (akan memudar saat diaduk)")]
    public Transform rawIngredientsContainer;

    [Header("Visual States (Diisi dari StirManager saat runtime)")]
    [Tooltip("Daftar stage yang sudah di-setup. Tidak perlu diisi manual di sini.")]
    public LayeredCookingStage[] layeredCookingStages;

    // Daftar renderer yang aktif untuk transisi (langsung dari scene, bukan clone)
    private List<SpriteRenderer> activeBaseRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> activeFrontRenderers = new List<SpriteRenderer>();

    /// <summary>
    /// Dipanggil oleh StirManager saat fase stir dimulai.
    /// Mengambil referensi langsung ke GameObject yang sudah ada di scene.
    /// TIDAK ada Instantiate — hanya enable dan atur alpha.
    /// </summary>
    public void SetLayeredTransitionStages(LayeredCookingStage[] newStages)
    {
        layeredCookingStages = newStages;
        activeBaseRenderers.Clear();
        activeFrontRenderers.Clear();

        for (int i = 0; i < newStages.Length; i++)
        {
            // Base renderer
            if (newStages[i].baseRenderer != null)
            {
                SpriteRenderer bsr = newStages[i].baseRenderer;
                bsr.gameObject.SetActive(true);   // Aktifkan GameObject
                bsr.enabled = true;
                bsr.color = new Color(1f, 1f, 1f, 0f); // Mulai dari transparan
                activeBaseRenderers.Add(bsr);
            }

            // Front renderer
            if (newStages[i].frontRenderer != null)
            {
                SpriteRenderer fsr = newStages[i].frontRenderer;
                fsr.gameObject.SetActive(true);   // Aktifkan GameObject
                fsr.enabled = true;
                fsr.color = new Color(1f, 1f, 1f, 0f); // Mulai dari transparan
                activeFrontRenderers.Add(fsr);
            }
        }

        Debug.Log($"[CookingVisual] SetLayeredTransitionStages: {newStages.Length} stages, base={activeBaseRenderers.Count}, front={activeFrontRenderers.Count}");
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
    public void SetFoodVisual(int stageIndex)
    {
        if (layeredCookingStages == null || stageIndex < 0 || stageIndex >= layeredCookingStages.Length) return;

        // Sembunyikan semua stage dulu
        for (int i = 0; i < layeredCookingStages.Length; i++)
        {
            if (layeredCookingStages[i].baseRenderer != null)
                layeredCookingStages[i].baseRenderer.color = new Color(1f, 1f, 1f, 0f);
            if (layeredCookingStages[i].frontRenderer != null)
                layeredCookingStages[i].frontRenderer.color = new Color(1f, 1f, 1f, 0f);
        }

        // Tampilkan stage yang diminta
        if (layeredCookingStages[stageIndex].baseRenderer != null)
        {
            layeredCookingStages[stageIndex].baseRenderer.gameObject.SetActive(true);
            layeredCookingStages[stageIndex].baseRenderer.color = Color.white;
        }
        if (layeredCookingStages[stageIndex].frontRenderer != null)
        {
            layeredCookingStages[stageIndex].frontRenderer.gameObject.SetActive(true);
            layeredCookingStages[stageIndex].frontRenderer.color = Color.white;
        }
    }

    public void SetCookingStage(int stageIndex)
    {
        SetFoodVisual(stageIndex);
    }

    // --- LOGIKA PROGRESS ADUKAN ---
    public void UpdateStirProgress(float progress)
    {
        int stageCount = layeredCookingStages != null ? layeredCookingStages.Length : 0;
        
        // Scaled progress membagi 0->1 menjadi beberapa transisi berurutan.
        // Cth 2 stage: 0->1 (Raw ke Stage 0), 1->2 (Stage 0 ke Stage 1)
        float scaledProgress = progress * (stageCount == 0 ? 1 : stageCount); 

        // 1. Pudarkan bahan mentah yang jatuh (Hanya aktif selama interval pertama 0 -> 1)
        if (rawIngredientsContainer != null)
        {
            float rawAlpha = 1f;
            if (scaledProgress < 1f) rawAlpha = 1f - scaledProgress;
            else rawAlpha = 0f;

            foreach (SpriteRenderer sr in rawIngredientsContainer.GetComponentsInChildren<SpriteRenderer>())
            {
                Color colorA = sr.color; 
                colorA.a = rawAlpha; 
                sr.color = colorA;
            }
        }

        if (stageCount == 0) return;

        // 2. Transisi stage makanan secara berurutan
        for (int i = 0; i < stageCount; i++)
        {
            float targetAlpha = 0f;
            float peakProgress = i + 1f;

            if (scaledProgress >= i && scaledProgress <= peakProgress)
            {
                // Sedang Fade IN
                targetAlpha = scaledProgress - i;
            }
            else if (scaledProgress > peakProgress && scaledProgress <= peakProgress + 1f)
            {
                // Sedang Fade OUT ke stage berikutnya
                targetAlpha = 1f - (scaledProgress - peakProgress);
            }
            else
            {
                targetAlpha = 0f;
            }

            // Pastikan stage terakhir tetap full opacity di akhir
            if (i == stageCount - 1 && progress >= 1f)
            {
                targetAlpha = 1f;
            }

            if (i < activeBaseRenderers.Count && activeBaseRenderers[i] != null)
            {
                Color c = activeBaseRenderers[i].color;
                c.a = targetAlpha;
                activeBaseRenderers[i].color = c;
            }

            if (i < activeFrontRenderers.Count && activeFrontRenderers[i] != null)
            {
                Color c = activeFrontRenderers[i].color;
                c.a = targetAlpha;
                activeFrontRenderers[i].color = c;
            }
        }
    }
}
