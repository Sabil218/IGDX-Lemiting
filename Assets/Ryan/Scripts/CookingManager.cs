using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public interface ICookingPhase
{
    void StartPhase();
}

public class CookingManager : MonoBehaviour
{
    public static CookingManager instance { get; private set; }

    [System.Serializable]
    public class CookingPhase
    {
        public string phaseName;
        public GameObject phaseContainer;
        
        [Tooltip("Event/Fungsi yang akan dijalankan saat fase ini dimulai")]
        public UnityEngine.Events.UnityEvent onPhaseStart;
    }

    // ─── Inspector References ───────────────────────────────────────

    [Header("Cooking Sequence (Dinamis)")]
    [Tooltip("Urutan memasak. Tambahkan urutan dengan tombol +")]
    public System.Collections.Generic.List<CookingPhase> cookingSequence = new System.Collections.Generic.List<CookingPhase>();
    private int currentPhaseIndex = 0;

    //Initialize instance
    void Awake()
    {
        instance = this;
    }

    //Start first cooking phase
    void Start()
    {
        currentPhaseIndex = 0;
        if (cookingSequence.Count > 0)
        {
            PlayCurrentPhase();
        }
        else
        {
            Debug.LogWarning("No Container for Cooking Phase");
        }
    }

    //Load and setup active phase
    private void PlayCurrentPhase()
    {
        //Reset Container
        foreach (var phase in cookingSequence)
        {
            if (phase.phaseContainer != null) 
            {
                phase.phaseContainer.SetActive(false);
            }
        }

        if (currentPhaseIndex < cookingSequence.Count)
        {
            //Fase MiniGame Saat ini
            CookingPhase currentPhase = cookingSequence[currentPhaseIndex];
            
            Debug.Log($"[CookingManager] Memulai Fase: {currentPhase.phaseName}");

            //Show Container
            if (currentPhase.phaseContainer != null)
            {
                currentPhase.phaseContainer.SetActive(true);

                // Jalankan otomatis semua ICookingPhase yang ada di dalam container
                ICookingPhase[] phases = currentPhase.phaseContainer.GetComponentsInChildren<ICookingPhase>(true);
                foreach (var phase in phases)
                {
                    phase.StartPhase();
                }
            }

            currentPhase.onPhaseStart?.Invoke();
        }
    }

    // (Fungsi StartSlicingPhase dan StartSpellingPhase telah dipindahkan ke masing-masing manager)

    //Trigger stirring phase
    public void StartStirringPhase()
    {
        SetStirGameplayLogicAndUI(true);
    }

    //Toggle stirring specific components
    public void SetStirGameplayLogicAndUI(bool isActive)
    {
        GameObject container = null;
        if (currentPhaseIndex < cookingSequence.Count)
        {
            container = cookingSequence[currentPhaseIndex].phaseContainer;
        }

        if (container == null) return;

        MonoBehaviour[] scripts = container.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour script in scripts)
        {
            if (script is ICookingPhase cookingPhase)
            {
                script.enabled = isActive;
                if (isActive) cookingPhase.StartPhase();
            }
            else if (script.GetType().Name == "StirInput")
            {
                script.enabled = isActive;
            }
        }

        Canvas stirCanvas = container.GetComponentInChildren<Canvas>(true);
        if (stirCanvas != null) stirCanvas.gameObject.SetActive(isActive);
    }

    // ─── Slicing Phase ──────────────────────────────────────────────


    //Advance to next cooking sequence
    public void NextStep()
    {
        currentPhaseIndex++;

        if (currentPhaseIndex < cookingSequence.Count)
        {
            PlayCurrentPhase();
        }
        else
        {
            Debug.Log("[CookingManager] Semua fase memasak telah selesai!");
        }
    }
}