using UnityEngine;
using UnityEngine.Events;

public class CookingDragManager : MonoBehaviour, ICookingPhase
{
    [Header("Components")]
    public CookingDropZone dropZone;

    [Header("Free Phase Setup (Bebas)")]
    public GameObject[] objectsToActivateOnStart;

    [Header("Sequential Spawning (Berurutan)")]
    public bool spawnSequentially = false;
    public Transform spawnPoint;
    public GameObject[] sequentialIngredients;
    public IngredientDropCutscene dropCutscene;
    public Transform targetDropArea;

    [Header("Global Events")]
    public UnityEvent OnStepCompleted;

    private int currentIndex = 0;
    private GameObject currentSpawnedItem;

    // Initializes the phase and prepares the ingredients to be dragged.
    public void StartPhase()
    {
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
            if (objectsToActivateOnStart != null)
            {
                foreach (var obj in objectsToActivateOnStart)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }
        }
    }

    // Handles spawning ingredients, either all at once or one by one sequentially.
    private void SpawnNextIngredient()
    {
        if (currentIndex < sequentialIngredients.Length)
        {
            GameObject ing = sequentialIngredients[currentIndex];
            if (ing != null)
            {
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
                currentIndex++;
                SpawnNextIngredient();
            }
        }
        else
        {
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

    // Triggered when an ingredient is successfully dragged into the pan.
    private void HandleItemDropped(WorldObjectDraggable item)
    {
        if (spawnSequentially)
        {
            if (dropCutscene != null && targetDropArea != null)
            {
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

    private void HandleAllItemsReceived()
    {
        if (!spawnSequentially)
        {
            CompleteStep();
        }
    }

    // Completes this cooking phase and signals the global manager to advance.
    private void CompleteStep()
    {
        OnStepCompleted?.Invoke();
    }
}
