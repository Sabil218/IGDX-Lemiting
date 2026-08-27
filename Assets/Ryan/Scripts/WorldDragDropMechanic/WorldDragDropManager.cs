using UnityEngine;
using UnityEngine.Events;

public class WorldDragDropManager : MonoBehaviour, ICookingPhase
{
    [Header("Components")]
    public WorldDropZone dropZone;

    [Header("Phase Setup")]
    [Tooltip("Objek yang akan diaktifkan saat phase dimulai (misal: wajan, bahan mentah)")]
    public GameObject[] objectsToActivateOnStart;

    [Header("Global Events")]
    public UnityEvent OnStepCompleted;

    //Initialize Drag and Drop Phase
    public void StartPhase()
    {
        Debug.Log("[WorldDragDropManager] Memulai Drag & Drop Phase...");
        
        // Aktifkan objek yang dibutuhkan
        if (objectsToActivateOnStart != null)
        {
            foreach (var obj in objectsToActivateOnStart)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        // Reset Drop Zone jika tahapan diulang
        if (dropZone != null)
        {
            dropZone.ResetZone();
        }
    }

    //Subscribe to drop event
    private void Start()
    {
        if (dropZone != null)
        {
            dropZone.OnAllItemsReceived.AddListener(HandleAllItemsReceived);
        }
    }

    //Unsubscribe from drop event
    private void OnDestroy()
    {
        if (dropZone != null)
        {
            dropZone.OnAllItemsReceived.RemoveListener(HandleAllItemsReceived);
        }
    }

    //Process logic when all items are collected
    private void HandleAllItemsReceived()
    {
        Debug.Log("[WorldDragDropManager] Memproses penyelesaian tahap...");
        CompleteStep();
    }

    //Trigger stage completion event
    private void CompleteStep()
    {
        Debug.Log("[WorldDragDropManager] Tahap Selesai! Memanggil event OnStepCompleted.");
        OnStepCompleted?.Invoke();
    }
}
