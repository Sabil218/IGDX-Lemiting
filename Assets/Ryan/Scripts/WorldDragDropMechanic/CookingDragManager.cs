using UnityEngine;
using UnityEngine.Events;

public class CookingDragManager : MonoBehaviour, ICookingPhase
{
    [Header("Components")]
    public CookingDropZone dropZone;

    [Header("Free Phase Setup (Bebas)")]
    [Tooltip("Objek yang akan diaktifkan secara bebas sekaligus saat phase dimulai (Non-Sequential)")]
    public GameObject[] objectsToActivateOnStart;

    [Header("Sequential Spawning (Berurutan)")]
    [Tooltip("Jika dicentang, akan menggunakan mode memanggil bahan satu per satu.")]
    public bool spawnSequentially = false;
    public Transform spawnPoint; // Titik orange
    [Tooltip("Daftar bahan makanan (Prefab atau objek di dalam Scene yang nonaktif)")]
    public GameObject[] sequentialIngredients;
    public IngredientDropCutscene dropCutscene;
    [Tooltip("Target area wajan")]
    public Transform targetDropArea;

    [Header("Global Events")]
    public UnityEvent OnStepCompleted;

    private int currentIndex = 0;
    private GameObject currentSpawnedItem;

    //Initialize Drag and Drop Phase
    public void StartPhase()
    {
        // Reset Drop Zone jika tahapan diulang
        if (dropZone != null)
        {
            dropZone.ResetZone();
        }

        if (spawnSequentially)
        {
            currentIndex = 0;
            SpawnNextIngredient();
        }
        else
        {
            // Mode Lama: Aktifkan semua bebas
            if (objectsToActivateOnStart != null)
            {
                foreach (var obj in objectsToActivateOnStart)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }
        }
    }

    private void SpawnNextIngredient()
    {
        if (currentIndex < sequentialIngredients.Length)
        {
            GameObject ing = sequentialIngredients[currentIndex];
            if (ing != null)
            {
                // Cek apakah ini objek dari Scene atau sebuah Prefab
                if (ing.scene.IsValid()) 
                {
                    currentSpawnedItem = ing;
                    if (spawnPoint != null) currentSpawnedItem.transform.position = spawnPoint.position;
                    currentSpawnedItem.SetActive(true);
                }
                else
                {
                    Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
                    currentSpawnedItem = Instantiate(ing, pos, Quaternion.identity, transform);
                    currentSpawnedItem.SetActive(true);
                }
            }
            else
            {
                // Skip jika ada slot kosong
                currentIndex++;
                SpawnNextIngredient();
            }
        }
        else
        {
            // Semua bahan berurutan selesai diproses
            CompleteStep();
        }
    }

    private void Start()
    {
        if (dropZone != null)
        {
            dropZone.OnAllItemsReceived.AddListener(HandleAllItemsReceived);
            dropZone.OnItemDroppedAction += HandleItemDropped;
        }
    }

    private void OnDestroy()
    {
        if (dropZone != null)
        {
            dropZone.OnAllItemsReceived.RemoveListener(HandleAllItemsReceived);
            dropZone.OnItemDroppedAction -= HandleItemDropped;
        }
    }

    private void HandleItemDropped(IngredientDraggable item)
    {
        if (spawnSequentially)
        {
            if (dropCutscene != null && targetDropArea != null)
            {
                // Putar cutscene, dan setelah cutscene selesai, panggil bahan berikutnya
                dropCutscene.Play(item.transform, targetDropArea, null, OnCutsceneFinished);
            }
            else
            {
                OnCutsceneFinished();
            }
        }
    }

    private void OnCutsceneFinished()
    {
        currentIndex++;
        SpawnNextIngredient();
    }

    //Process logic when all items are collected (Mode Non-Sequential)
    private void HandleAllItemsReceived()
    {
        if (!spawnSequentially)
        {
            CompleteStep();
        }
    }

    //Trigger stage completion event
    private void CompleteStep()
    {
        OnStepCompleted?.Invoke();
    }
}
