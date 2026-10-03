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

    // Memulai fase plating dan animasi masuk wajan & piring
    public void StartPhase()
    {
        // 1. Pindah kamera ke meja saji jika ada
        if (platingVirtualCamera != null)
        {
            platingVirtualCamera.SetActive(true);
            var vcam = platingVirtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
            if (vcam != null) vcam.Priority = 30;
        }

        // Pastikan sendok pengaduk dan api kompor mati saat masuk fase plating
        CookingManager.SetStoveFireActive(false);
        CookingManager.SetSpoonActive(false);

        // Matikan CookedDummy jika ada di scene
        GameObject autoDummy = GameObject.Find("CookedDummy");
        if (autoDummy != null)
        {
            autoDummy.SetActive(false);
        }

        // 2. Setup cutscene player dan kunci drag wajan sementara
        if (cutscenePlayer != null)
        {
            cutscenePlayer.gameObject.SetActive(true);
        }
        
        if (wajanDraggable != null) wajanDraggable.enabled = false;

        // 3. Mainkan animasi wajan & piring masuk bersamaan seperti versi lama
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

        // 4. Daftarkan event saat wajan dituang ke piring
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

    // Dipanggil saat wajan berhasil di-drop ke atas piring
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
