using System.Collections.Generic;
using UnityEngine;

public class WordMatchingManager : MonoBehaviour, ICookingPhase
{
    public static WordMatchingManager instance { get; private set; }

    [System.Serializable]
    public class SpellingRoundData
    {
        [Tooltip("Kata yang dieja")]
        public string targetWord;
        [Tooltip("Objek bahan makanan")]
        public Transform ingredientObject;
    }

    [Header("Spelling Rounds")]
    [Tooltip("Daftar kata yang harus dieja")]
    public SpellingRoundData[] spellingRounds;
    private int currentSpellingIndex = 0;

    [Header("Spelling Cutscene")]
    [Tooltip("Referensi skrip cutscene")]
    public IngredientDropCutscene dropCutscene;
    [Tooltip("Target area wajan (Harus memiliki Collider2D)")]
    public Collider2D targetDropArea;
    [Tooltip("Container tempat potongan akan dijatuhkan (Di dalam Wajan Global)")]
    public Transform panContentContainer;

    [Header("Events")]
    [Tooltip("Event ini dipanggil saat seluruh ronde ejaan tamat.")]
    public UnityEngine.Events.UnityEvent onSpellingComplete;

    [Header("Prefabs")]
    [SerializeField] private GameObject AnswerArea;
    [SerializeField] private GameObject WordTile;

    [Header("Scene Container")]
    [SerializeField] private Transform targetBoardContainer; // Area Jawaban
    [SerializeField] private Transform letterPoolContainer; // Area Shuffled Letter
    [SerializeField] private int maxCols = 5;

    // Object Pool Letter Tile
    private Queue<GameObject> letterTilePool = new Queue<GameObject>();

    private string currentWord;
    private Transform currentIngredient;

    // Store Spelled Word
    private BoardDropZone mainWordDisplay;

    // Store Spawned Ingredient
    private GameObject currentSpawnedIngredient;

    private void Awake()
    {
        instance = this;
        Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
        Camera.main.transparencySortAxis = new Vector3(0, 1, 0);
    }

    public void StartPhase()
    {
        if (panContentContainer == null && CookingVisualController.Instance != null)
        {
            panContentContainer = CookingVisualController.Instance.rawIngredientsContainer;
        }

        currentSpellingIndex = 0;
        LoadCurrentSpellingRound();
    }

    private void LoadCurrentSpellingRound()
    {
        if (spellingRounds == null || spellingRounds.Length == 0)
        {
            Debug.LogWarning("[WordMatchingManager] No spelling rounds configured!");
            onSpellingComplete?.Invoke();
            return;
        }

        if (currentSpellingIndex < spellingRounds.Length)
        {
            SpellingRoundData round = spellingRounds[currentSpellingIndex];
            Transform activeIngredient = round.ingredientObject;

            if (activeIngredient != null)
            {
                if (!activeIngredient.gameObject.scene.IsValid())
                {
                    GameObject spawned = Instantiate(activeIngredient.gameObject, transform.position, transform.rotation, transform);
                    currentSpawnedIngredient = spawned;
                    activeIngredient = spawned.transform;
                }

                activeIngredient.gameObject.SetActive(false);

                FoodSlicer slicer = activeIngredient.GetComponent<FoodSlicer>();
                if (slicer != null)
                {
                    slicer.ForceSlicedState();
                }
            }

            LoadRound(round.targetWord, activeIngredient);
        }
    }

    public void LoadRound(string word, Transform ingredient)
    {
        currentWord = word.ToUpper();
        currentIngredient = ingredient;

        ClearContainer(targetBoardContainer);
        ClearLetterPool();

        GameObject wordObj = Instantiate(AnswerArea, targetBoardContainer, false);
        wordObj.transform.localPosition = new Vector3(wordObj.transform.localPosition.x, wordObj.transform.localPosition.y, 0f);
        mainWordDisplay = wordObj.GetComponent<BoardDropZone>();
        if (mainWordDisplay != null)
        {
            mainWordDisplay.InitializeWord(currentWord);
        }

        List<char> charactersToSpawn = new List<char>();
        foreach (char c in currentWord)
        {
            if (c != ' ') charactersToSpawn.Add(c);
        }

        string cleanTargetWord = new string(charactersToSpawn.ToArray());
        int attempts = 0;
        do
        {
            ShuffleList(charactersToSpawn);
            attempts++;
        }
        while (new string(charactersToSpawn.ToArray()) == cleanTargetWord && attempts < 10);

        Transform currentRow = null;

        for (int i = 0; i < charactersToSpawn.Count; i++)
        {
            if (i % maxCols == 0)
            {
                GameObject rowObj = new GameObject("Row_" + (i / maxCols), typeof(RectTransform));
                rowObj.layer = letterPoolContainer.gameObject.layer;
                rowObj.transform.SetParent(letterPoolContainer, false);
                rowObj.transform.localScale = Vector3.one;
                rowObj.transform.localPosition = Vector3.zero;

                UnityEngine.UI.HorizontalLayoutGroup hg = rowObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                hg.childAlignment = TextAnchor.MiddleCenter;
                hg.spacing = 20;
                hg.childControlWidth = false;
                hg.childControlHeight = false;
                hg.childForceExpandWidth = false;
                hg.childForceExpandHeight = false;

                currentRow = rowObj.transform;
            }

            GameObject tileObj;
            if (letterTilePool.Count > 0)
            {
                tileObj = letterTilePool.Dequeue();
                tileObj.transform.SetParent(currentRow, false);
                tileObj.SetActive(true);
            }
            else
            {
                tileObj = Instantiate(WordTile, currentRow, false);
            }

            tileObj.transform.localPosition = new Vector3(tileObj.transform.localPosition.x, tileObj.transform.localPosition.y, 0f);

            DraggableLetterTile tile = tileObj.GetComponent<DraggableLetterTile>();
            tile.Initialize(charactersToSpawn[i]);
        }
    }

    public void OnLetterCorrectlySpelled()
    {
        RepackTiles();
    }

    public void OnWordComplete()
    {
        StartCoroutine(HandleWordComplete());
    }

    private void RepackTiles()
    {
        List<Transform> activeTiles = new List<Transform>();
        List<Transform> activeRows = new List<Transform>();

        foreach (Transform row in letterPoolContainer)
        {
            if (row.GetComponent<DraggableLetterTile>() != null) continue;

            activeRows.Add(row);

            foreach (Transform tile in row)
            {
                DraggableLetterTile dlt = tile.GetComponent<DraggableLetterTile>();
                if (dlt != null && !dlt.isConsumed)
                {
                    activeTiles.Add(tile);
                }
            }
        }
        for (int i = 0; i < activeTiles.Count; i++)
        {
            int rowIndex = i / maxCols;
            if (rowIndex < activeRows.Count)
            {
                activeTiles[i].SetParent(activeRows[rowIndex], false);
            }
        }
    }

    private System.Collections.IEnumerator HandleWordComplete()
    {
        yield return new WaitForSeconds(0.8f);
        ProcessRoundCompletion();
    }

    private void ProcessRoundCompletion()
    {
        if (spellingRounds == null || spellingRounds.Length == 0 || currentSpellingIndex >= spellingRounds.Length)
            return;

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
            if (cg == null) cg = canvas.gameObject.AddComponent<CanvasGroup>();
            StartCoroutine(FadeCanvasGroup(cg, 0f, 0.5f, () => canvas.gameObject.SetActive(false)));
        }

        if (dropCutscene != null && currentIngredient != null && targetDropArea != null)
        {
            dropCutscene.Play(currentIngredient, targetDropArea, panContentContainer, () =>
            {
                // Bagian ini sekarang hanya memanggil next round tanpa menghapus objek bahan!
                AdvanceSpellingRound();
            });
        }
        else
        {
            Debug.LogWarning("[WordMatchingManager] DropCutscene, targetDropArea, or ingredient not assigned — skipping cutscene.");
            AdvanceSpellingRound();
        }
    }

    private void AdvanceSpellingRound()
    {
        currentSpellingIndex++;
        StartCoroutine(ResetCameraAndNextRound());
    }

    private System.Collections.IEnumerator ResetCameraAndNextRound()
    {
        yield return new WaitForSeconds(0.8f);

        if (currentSpellingIndex < spellingRounds.Length)
        {
            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
                CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }

            LoadCurrentSpellingRound();
        }
        else
        {
            onSpellingComplete?.Invoke();
        }
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearLetterPool()
    {
        if (letterPoolContainer == null) return;

        for (int i = letterPoolContainer.childCount - 1; i >= 0; i--)
        {
            Transform row = letterPoolContainer.GetChild(i);

            if (row.GetComponent<DraggableLetterTile>() != null) continue;

            for (int j = row.childCount - 1; j >= 0; j--)
            {
                Transform tile = row.GetChild(j);
                tile.gameObject.SetActive(false);
                tile.SetParent(letterPoolContainer, false);
                letterTilePool.Enqueue(tile.gameObject);
            }
            Destroy(row.gameObject);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, System.Action onComplete)
    {
        float startAlpha = cg.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}