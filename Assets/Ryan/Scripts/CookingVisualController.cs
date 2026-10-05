using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct LayeredCookingStage
{
    public SpriteRenderer baseRenderer;
    public SpriteRenderer frontRenderer;
}

public class CookingVisualController : MonoBehaviour
{
    public static CookingVisualController Instance { get; private set; }

    [Header("Visual Components")]
    public SpriteRenderer panRenderer;
    public SpriteRenderer foodContentRenderer;
    public SpriteRenderer frontOverlayRenderer;
    public Transform rawIngredientsContainer;

    [Header("Visual States (Diisi dari StirManager saat runtime)")]
    public LayeredCookingStage[] layeredCookingStages;

    private List<SpriteRenderer> activeBaseRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> activeFrontRenderers = new List<SpriteRenderer>();

    // Prepares the sprite layers for cooking progression and hides visuals from previous phases.
    public void SetLayeredTransitionStages(LayeredCookingStage[] newStages)
    {
        ResetAllCookedVisuals();

        layeredCookingStages = newStages;
        activeBaseRenderers.Clear();
        activeFrontRenderers.Clear();

        if (newStages != null)
        {
            for (int i = 0; i < newStages.Length; i++)
            {
                if (newStages[i].baseRenderer != null)
                {
                    SpriteRenderer bsr = newStages[i].baseRenderer;
                    if (bsr.transform.parent != null && !bsr.transform.parent.gameObject.activeSelf)
                    {
                        bsr.transform.parent.gameObject.SetActive(true);
                    }
                    bsr.gameObject.SetActive(true);
                    bsr.enabled = true;
                    bsr.color = new Color(1f, 1f, 1f, 0f);
                    activeBaseRenderers.Add(bsr);
                }

                if (newStages[i].frontRenderer != null)
                {
                    SpriteRenderer fsr = newStages[i].frontRenderer;
                    if (fsr.transform.parent != null && !fsr.transform.parent.gameObject.activeSelf)
                    {
                        fsr.transform.parent.gameObject.SetActive(true);
                    }
                    fsr.gameObject.SetActive(true);
                    fsr.enabled = true;
                    fsr.color = new Color(1f, 1f, 1f, 0f);
                    activeFrontRenderers.Add(fsr);
                }
            }
        }

        // Reset progress ke 0 agar bahan mentah tampak 100% dan layer matang transparan 0%
        UpdateStirProgress(0f);

        Debug.Log($"[CookingVisual] SetLayeredTransitionStages: {newStages?.Length ?? 0} stages, base={activeBaseRenderers.Count}, front={activeFrontRenderers.Count}");
    }

    public void ResetAllCookedVisuals()
    {
        // 1. Matikan dan transparankan semua layer Papeda
        Transform pMatang = transform.Find("PapedaMatang");
        if (pMatang != null)
        {
            foreach (var sr in pMatang.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.color = new Color(1f, 1f, 1f, 0f);
            }
            pMatang.gameObject.SetActive(false);
        }

        Transform pSetengah = transform.Find("PapedaSetengah");
        if (pSetengah != null)
        {
            foreach (var sr in pSetengah.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.color = new Color(1f, 1f, 1f, 0f);
            }
            pSetengah.gameObject.SetActive(false);
        }

        // 2. Matikan dan transparankan semua layer Kuah
        Transform kMatang = transform.Find("Matang");
        if (kMatang != null)
        {
            foreach (var sr in kMatang.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.color = new Color(1f, 1f, 1f, 0f);
            }
            kMatang.gameObject.SetActive(false);
        }

        Transform kSetengah = transform.Find("SetengahMatang");
        if (kSetengah != null)
        {
            foreach (var sr in kSetengah.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.color = new Color(1f, 1f, 1f, 0f);
            }
            kSetengah.gameObject.SetActive(false);
        }

        foreach (SpriteRenderer r in activeBaseRenderers) {
            if (r != null) r.color = new Color(1f, 1f, 1f, 0f);
        }
        foreach (SpriteRenderer r in activeFrontRenderers) {
            if (r != null) r.color = new Color(1f, 1f, 1f, 0f);
        }

        layeredCookingStages = null;
        activeBaseRenderers.Clear();
        activeFrontRenderers.Clear();
    }

    public void ClearRawIngredients()
    {
        if (rawIngredientsContainer != null)
        {
            for (int i = rawIngredientsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = rawIngredientsContainer.GetChild(i);
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ResetAllCookedVisuals();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Instantly sets the food visual to a specific cooking stage (e.g., Raw, Cooked, Burnt).
    public void SetFoodVisual(int stageIndex)
    {
        if (layeredCookingStages == null || stageIndex < 0 || stageIndex >= layeredCookingStages.Length) return;

        for (int i = 0; i < layeredCookingStages.Length; i++)
        {
            if (layeredCookingStages[i].baseRenderer != null)
                layeredCookingStages[i].baseRenderer.color = new Color(1f, 1f, 1f, 0f);
            if (layeredCookingStages[i].frontRenderer != null)
                layeredCookingStages[i].frontRenderer.color = new Color(1f, 1f, 1f, 0f);
        }

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

    // Alias method for SetFoodVisual used by external scripts.
    public void SetCookingStage(int stageIndex)
    {
        SetFoodVisual(stageIndex);
    }

    // Smoothly crossfades between raw ingredients and cooked layers based on stirring progress (0.0 to 1.0).
    public void UpdateStirProgress(float progress)
    {
        int stageCount = layeredCookingStages != null ? layeredCookingStages.Length : 0;
        
        float scaledProgress = progress * (stageCount == 0 ? 1 : stageCount); 

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

        for (int i = 0; i < stageCount; i++)
        {
            float targetAlpha = 0f;
            float peakProgress = i + 1f;

            if (scaledProgress >= i && scaledProgress <= peakProgress)
            {
                targetAlpha = scaledProgress - i;
            }
            else if (scaledProgress > peakProgress && scaledProgress <= peakProgress + 1f)
            {
                targetAlpha = 1f - (scaledProgress - peakProgress);
            }
            else
            {
                targetAlpha = 0f;
            }

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
