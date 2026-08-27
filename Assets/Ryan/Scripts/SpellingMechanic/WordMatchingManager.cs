using System.Collections.Generic;
using System.Linq;
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
    [Tooltip("Titik tengah wajan (target jatuh)")]
    public Transform panCenterPoint;

    [Header("Events")]
    [Tooltip("Event ini dipanggil saat seluruh ronde ejaan tamat.")]
    public UnityEngine.Events.UnityEvent onSpellingComplete;

    [Header("Prefabs")]
    [SerializeField] private GameObject AnswerArea;
    [SerializeField] private GameObject WordTile;

    [Header("Scene Container")]
    [SerializeField] private Transform targetBoardContainer; //Area Jawaban
    [SerializeField] private Transform letterPoolContainer; //Area Shuffled Letter
    [SerializeField] private int maxCols = 5;

    // Object Pool Letter Tile
    private Queue<GameObject> letterTilePool = new Queue<GameObject>();

    private string currentWord;
    private Transform currentIngredient;
    
    // Store Spelled Word
    private BoardDropZone mainWordDisplay;
    
    // Store Spawned Ingredient
    private GameObject currentSpawnedIngredient;

    //Initialize instance and camera sorting
    private void Awake()
    {
        instance = this;
        Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
        Camera.main.transparencySortAxis = new Vector3(0, 1, 0);
    }

    //Start the spelling phase
    public void StartPhase()
    {
        currentSpellingIndex = 0;
        LoadCurrentSpellingRound();
    }

    //Load configuration for current spelling round
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
                if (!activeIngredient.gameObject.scene.IsValid()) // Jika berupa Prefab
                {
GameObject spawned = Instantiate(activeIngredient.gameObject, transform.position, transform.rotation, transform);                    currentSpawnedIngredient = spawned;
                    activeIngredient = spawned.transform;
                }

                activeIngredient.gameObject.SetActive(false);

                // Auto-convert to sliced state if it's a whole ingredient
                FoodSlicer slicer = activeIngredient.GetComponent<FoodSlicer>();
                if (slicer != null)
                {
                    slicer.ForceSlicedState();
                }
            }

            LoadRound(round.targetWord, activeIngredient);
        }
    }

    //Initialize UI and pool for current word
    public void LoadRound(string word, Transform ingredient)
    {
        currentWord = word.ToUpper();
        currentIngredient = ingredient;

        ClearContainer(targetBoardContainer);
        ClearLetterPool(); // Gunakan fungsi pembersih khusus Pool

        // BUG FIX: Instantiate UI wajib menggunakan parameter false agar Z-position tidak corrupt!
        GameObject wordObj = Instantiate(AnswerArea, targetBoardContainer, false);
        wordObj.transform.localPosition = new Vector3(wordObj.transform.localPosition.x, wordObj.transform.localPosition.y, 0f);
        mainWordDisplay = wordObj.GetComponent<BoardDropZone>();
        if (mainWordDisplay != null)
        {
            mainWordDisplay.InitializeWord(currentWord);
        }

        //Generate list huruf
        List<char> charactersToSpawn = new List<char>();
        foreach (char c in currentWord)
        {
            if (c != ' ') charactersToSpawn.Add(c); //Ignore White Space
        }
        
        // Coba shuffle maksimal 10 kali agar tidak kebetulan membentuk kata aslinya
        string cleanTargetWord = new string(charactersToSpawn.ToArray());
        int attempts = 0;
        do
        {
            ShuffleList(charactersToSpawn);
            attempts++;
        } 
        while (new string(charactersToSpawn.ToArray()) == cleanTargetWord && attempts < 10);

        Transform currentRow = null;

        //Layout Tile Pool
        for (int i = 0; i < charactersToSpawn.Count; i++)
        {
            // Buat baris baru setiap kelipatan maxCols
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

            //Visual Bug Fix
            tileObj.transform.localPosition = new Vector3(tileObj.transform.localPosition.x, tileObj.transform.localPosition.y, 0f);

            DraggableLetterTile tile = tileObj.GetComponent<DraggableLetterTile>();
            tile.Initialize(charactersToSpawn[i]);
        }
    }

    //Re-layout remaining tiles
    public void OnLetterCorrectlySpelled()
    {
        RepackTiles();
    }

    //Trigger word completion cutscene sequence
    public void OnWordComplete()
    {
        Debug.Log("[WordMatchingManager] Kata selesai! Menjalankan cutscene...");
        StartCoroutine(HandleWordComplete());
    }

    //Re-arrange unused tiles into grid
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

    //Delay before cutscene
    private System.Collections.IEnumerator HandleWordComplete()
    {
        yield return new WaitForSeconds(0.8f);
        ProcessRoundCompletion();
    }

    //Play drop cutscene and hide UI
    private void ProcessRoundCompletion()
    {
        if (spellingRounds == null || spellingRounds.Length == 0 || currentSpellingIndex >= spellingRounds.Length)
            return;

        SpellingRoundData round = spellingRounds[currentSpellingIndex];

        // Sembunyikan UI Canvas sementara selama cutscene
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null) canvas.gameObject.SetActive(false);

        Vector3 panPos = panCenterPoint != null ? panCenterPoint.position : Vector3.zero;

        if (dropCutscene != null && currentIngredient != null)
        {
            dropCutscene.Play(currentIngredient, panPos, () =>
            {
                AdvanceSpellingRound();
            });
        }
        else
        {
            Debug.LogWarning("[WordMatchingManager] DropCutscene or ingredient not assigned — skipping cutscene.");
            AdvanceSpellingRound();
        }
    }

    //Move to next spelling round
    private void AdvanceSpellingRound()
    {
        currentSpellingIndex++;
        StartCoroutine(ResetCameraAndNextRound());
    }

    //Delay and load next round or finish phase
    private System.Collections.IEnumerator ResetCameraAndNextRound()
    {
        yield return new WaitForSeconds(0.8f);

        if (currentSpellingIndex < spellingRounds.Length)
        {
            // Nyalakan UI Canvas kembali
            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null) canvas.gameObject.SetActive(true);
            
            LoadCurrentSpellingRound();
        }
        else
        {
            Debug.Log("[WordMatchingManager] Seluruh proses Spelling selesai!");
            onSpellingComplete?.Invoke();
        }
    }

    //Destroy all child objects
    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    //Return tiles to pool and destroy rows
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

            Destroy(row.gameObject); // Hancurkan kerangka baris kosong
        }
    }

    //Randomize list elements
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
}
