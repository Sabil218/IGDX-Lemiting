using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlatingManager : MonoBehaviour, ICookingPhase
{
    [Header("Phase References")]
    public PlatingCutscene cutscenePlayer;
    public WorldObjectDraggable wajanDraggable;
    public CookingDropZone piringDropZone;

    [Header("Camera Transition")]
    public GameObject platingVirtualCamera;

    [Header("Events")]
    public UnityEvent OnPhaseComplete;

    // Starts the plating sequence and triggers the entry animation.
    public void StartPhase()
    {
        if (platingVirtualCamera != null)
        {
            platingVirtualCamera.SetActive(true);
        }

        GameObject autoDummy = GameObject.Find("CookedDummy");
        if (autoDummy != null)
        {
            autoDummy.SetActive(false);
        }

        if (cutscenePlayer != null)
        {
            cutscenePlayer.gameObject.SetActive(true);
        }
        
        if (wajanDraggable != null) wajanDraggable.enabled = false;

        if (cutscenePlayer != null)
        {
            cutscenePlayer.PlayEntryAnimation(() => 
            {
                if (wajanDraggable != null) wajanDraggable.enabled = true;
            });
        }
        else
        {
            if (wajanDraggable != null) wajanDraggable.enabled = true;
        }

        if (piringDropZone != null)
        {
            piringDropZone.ResetZone();
            piringDropZone.OnAllItemsReceived.AddListener(HandleDropReceived);
        }
    }

    private void OnDestroy()
    {
        if (piringDropZone != null)
        {
            piringDropZone.OnAllItemsReceived.RemoveListener(HandleDropReceived);
        }
    }

    // Triggered when the player successfully drags the pan to the plate.
    private void HandleDropReceived()
    {
        if (wajanDraggable != null) wajanDraggable.enabled = false;

        if (cutscenePlayer != null)
        {
            cutscenePlayer.PlayPostDropAndExitAnimation(() => 
            {
                EndPhase();
            });
        }
        else
        {
            EndPhase();
        }
    }

    private void EndPhase()
    {
        OnPhaseComplete?.Invoke();

        if (CookingManager.instance != null)
        {
            CookingManager.instance.NextStep();
        }
    }
}
