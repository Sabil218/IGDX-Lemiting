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
        public UnityEngine.Events.UnityEvent onPhaseStart;
    }

    // ─── Inspector References ───────────────────────────────────────

    [Header("Cooking Sequence")]
    public System.Collections.Generic.List<CookingPhase> cookingSequence = new System.Collections.Generic.List<CookingPhase>();
    private int currentPhaseIndex = 0;

    //Initialize instance
    void Awake()
    {
        instance = this;
        Time.timeScale = 1f;
    }

    //Helper to control stove fire outside stirring phase
    public static void SetStoveFireActive(bool active)
    {
        GameObject stove = GameObject.Find("StoveObject");
        if (stove != null)
        {
            Transform fire = stove.transform.Find("Fire");
            if (fire != null && fire.gameObject.activeSelf != active)
            {
                fire.gameObject.SetActive(active);
            }
        }
    }

    //Helper to control spoon/spatula outside stirring phase
    public static void SetSpoonActive(bool active)
    {
        GameObject stove = GameObject.Find("StoveObject");
        if (stove != null)
        {
            Transform spoon = stove.transform.Find("Spoon");
            if (spoon != null && spoon.gameObject.activeSelf != active)
            {
                spoon.gameObject.SetActive(active);
            }
        }
    }

    //Start first cooking phase
    void Start()
    {
        Time.timeScale = 1f;
        Debug.Log("Cooking Started");
        currentPhaseIndex = 0;
        SetStoveFireActive(false);
        SetSpoonActive(false);
        if (cookingSequence.Count > 0)
        {
            PlayCurrentPhase();
        }
        else
        {
            Debug.LogWarning("No Cooking Container Reference Found");
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

        SetStoveFireActive(false);
        SetSpoonActive(false);

        if (currentPhaseIndex < cookingSequence.Count)
        {
            //Fase saat ini
            CookingPhase currentPhase = cookingSequence[currentPhaseIndex];
      
            //Show Container
            if (currentPhase.phaseContainer != null)
            {
                currentPhase.phaseContainer.SetActive(true);

                //Run all Phase Scripts that implement ICookingPhase
                ICookingPhase[] phases = currentPhase.phaseContainer.GetComponentsInChildren<ICookingPhase>(true);
                foreach (var phase in phases)
                {
                    phase.StartPhase();
                }
            }

            currentPhase.onPhaseStart?.Invoke();
        }
    }

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
            Debug.Log("All cooking phases completed.");
        }
    }
}