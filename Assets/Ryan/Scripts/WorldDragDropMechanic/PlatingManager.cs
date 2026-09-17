using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlatingManager : MonoBehaviour, ICookingPhase
{
    [Header("Phase References")]
    [Tooltip("Komponen yang mengatur jalannya animasi & visual (tarik script PlatingCutscene ke sini)")]
    public PlatingCutscene cutscenePlayer;
    
    [Tooltip("Draggable item wajan untuk menghidupkan/mematikan drag")]
    public WorldObjectDraggable wajanDraggable;
    
    [Tooltip("Drop zone piring untuk mendeteksi wajan")]
    public CookingDropZone piringDropZone;

    [Header("Camera Transition")]
    [Tooltip("Virtual Camera untuk phase ini. Biarkan kosong jika tidak pakai kamera khusus.")]
    public GameObject platingVirtualCamera;

    [Header("Events")]
    public UnityEvent OnPhaseComplete;

    public void StartPhase()
    {
        // 1. Pindah Kamera (Jika ada)
        if (platingVirtualCamera != null)
        {
            platingVirtualCamera.SetActive(true);
        }

        // [AUTO CLEAR] Mencari otomatis CookedDummy di scene dan mematikannya 
        GameObject autoDummy = GameObject.Find("CookedDummy");
        if (autoDummy != null)
        {
            autoDummy.SetActive(false);
        }

        // 2. Setup Cutscene & Nonaktifkan interaksi sementara
        if (cutscenePlayer != null)
        {
            cutscenePlayer.gameObject.SetActive(true);
        }
        
        if (wajanDraggable != null) wajanDraggable.enabled = false;

        // 3. Mainkan animasi Wajan & Piring masuk
        if (cutscenePlayer != null)
        {
            cutscenePlayer.PlayEntryAnimation(() => 
            {
                // Setelah selesai masuk, baru boleh di-drag
                if (wajanDraggable != null) wajanDraggable.enabled = true;
            });
        }
        else
        {
            // Fallback jika lupa memasang cutscene player
            if (wajanDraggable != null) wajanDraggable.enabled = true;
        }

        // 4. Daftarkan event tunggu drop
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

    private void HandleDropReceived()
    {
        // 5. Matikan interaksi lagi setelah drop sukses
        if (wajanDraggable != null) wajanDraggable.enabled = false;

        // 6. Mainkan efek jatuh, ganti sprite, & animasi keluar
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
        // 7. Selesaikan fase dan panggil NextStep
        OnPhaseComplete?.Invoke();

        if (CookingManager.instance != null)
        {
            CookingManager.instance.NextStep();
        }
    }
}
