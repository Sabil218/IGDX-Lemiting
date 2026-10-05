using System.Collections.Generic;
using UnityEngine;

public class WordMatchingManager : MonoBehaviour, ICookingPhase
{
    public static WordMatchingManager instance { get; private set; }

    [System.Serializable]
    public class SpellingRoundData
    {
        public string targetWord;
        public Transform ingredientObject;
        public bool disableRotation;
        public bool dropInCenter;
        [Tooltip("Jatuhkan potongan satu-per-satu secara berurutan ke wajan (jika false/default, bahan dijatuhkan langsung bersamaan seperti ikan).")]
        public bool dropSequentially;
        [Tooltip("Pengali ukuran khusus saat bahan muncul di rak eja (1 = ikuti prefab, misal 1.3 atau 1.5 jika ingin lebih besar di rak).")]
        public float spawnScaleMultiplier = 1f;
    }

    [Header("Spelling Rounds")]
    //Scale Food
    [SerializeField] private float defaultSpawnScaleMultiplier = 1f;
    public SpellingRoundData[] spellingRounds;
    private int currentSpellingIndex = 0;

    [Header("Spelling Cutscene")]
    public IngredientDropCutscene dropCutscene;
    public Transform targetDropArea;
    public Transform panContentContainer;
    // Titik awal di mana bahan makanan muncul (rak kayu) sebelum jatuh ke wajan.
    public Transform ingredientSpawnPoint;

    [Header("Cinemachine Cameras (Auto-detected if unassigned)")]
    [SerializeField] private Cinemachine.CinemachineVirtualCamera letterVirtualCamera;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera panVirtualCamera;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onSpellingComplete;
    public UnityEngine.Events.UnityEvent onDropCutsceneStart;
    public UnityEngine.Events.UnityEvent onDropCutsceneEnd;

    [Header("Transition Timings")]
    public float postDropWaitDelay = 0.8f;
    public float cameraReturnDelay = 1.0f;

    [Header("Audio / SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip correctLetterSfx;
    [SerializeField] private AudioClip wrongLetterSfx;
    [SerializeField] private AudioClip wordCompleteSfx;

    [Header("Prefabs")]
    [SerializeField] private GameObject AnswerArea;
    [SerializeField] private GameObject WordTile;

    [Header("Scene Container")]
    [SerializeField] private Transform targetBoardContainer;
    [SerializeField] private Transform letterPoolContainer;
    [SerializeField] private int maxCols = 6;
    private int activeCols = 6;

    private Queue<GameObject> letterTilePool = new Queue<GameObject>();

    private string currentWord;
    private Transform currentIngredient;
    private bool currentDisableRotation;
    private bool currentDropInCenter;
    private bool currentDropSequentially;

    private BoardDropZone mainWordDisplay;

    private GameObject currentSpawnedIngredient;

    private void Awake()
    {
        if (Camera.main != null)
        {
            Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
            Camera.main.transparencySortAxis = new Vector3(0, 1, 0);
        }
    }

    private void OnEnable()
    {
        instance = this;
    }

    private void Start()
    {
        EnsureCameraReferences();
        if (CookingManager.instance == null)
        {
            StartPhase();
        }
    }

    public void StartPhase()
    {
        EnsureCameraReferences();
        EnsureSpawnPointReference();
        EnsureDropAreaReference();

        if (letterVirtualCamera != null)
        {
            letterVirtualCamera.gameObject.SetActive(true);
            letterVirtualCamera.Priority = 15;
        }
        if (panVirtualCamera != null)
        {
            panVirtualCamera.Priority = 10;
        }

        if (panContentContainer == null && CookingVisualController.Instance != null)
        {
            panContentContainer = CookingVisualController.Instance.rawIngredientsContainer;
        }

        // Pastikan wajan bersih dari sisa bahan dan layer masakan fase sebelumnya (misal sagu / papeda)
        if (panContentContainer != null)
        {
            for (int i = panContentContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = panContentContainer.GetChild(i);
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
            }
        }

        if (CookingVisualController.Instance != null)
        {
            CookingVisualController.Instance.ResetAllCookedVisuals();
        }

        if (dropCutscene != null)
        {
            dropCutscene.ResetDropHistory();
        }

        currentSpellingIndex = 0;
        LoadCurrentSpellingRound();
    }

    // Initializes the next spelling round and spawns the raw ingredient.
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

                    // Terapkan pengali skala khusus saat di rak eja (agar proporsional tanpa mengubah ukuran di talenan)
                    float mult = round.spawnScaleMultiplier > 0.01f ? round.spawnScaleMultiplier : defaultSpawnScaleMultiplier;
                    if (mult > 0.01f && Mathf.Abs(mult - 1f) > 0.001f)
                    {
                        activeIngredient.localScale *= mult;
                    }
                }

                activeIngredient.gameObject.SetActive(false);

                FoodSlicer slicer = activeIngredient.GetComponent<FoodSlicer>();
                if (slicer != null)
                {
                    slicer.ForceSlicedState();
                }
            }

            LoadRound(round.targetWord, activeIngredient, round.disableRotation, round.dropInCenter, round.dropSequentially);
        }
    }

    [Header("Profanity Filter")]
    public string[] forbiddenWords = new string[] { "NIGA", "NIGGA", "FUCK", "SHIT", "CUNT", "ANJING", "ASU", "KONTOL", "MEMEK", "NGENTOT", "PELACUR", "BABI", "PORN", "SEX" };

    // Checks if the scrambled letters accidentally form an inappropriate word.
    private bool ContainsForbiddenWord(string text)
    {
        if (forbiddenWords == null || forbiddenWords.Length == 0) return false;
        
        string upperText = text.ToUpper();
        foreach (string badWord in forbiddenWords)
        {
            if (string.IsNullOrEmpty(badWord)) continue;
            if (upperText.Contains(badWord.ToUpper())) return true;
        }
        return false;
    }

    private bool WillFormForbiddenWord(string shuffled, string targetWord)
    {
        if (ContainsForbiddenWord(shuffled)) return true;

        string currentRemaining = shuffled;
        foreach (char c in targetWord)
        {
            int index = currentRemaining.IndexOf(c);
            if (index >= 0)
            {
                currentRemaining = currentRemaining.Remove(index, 1);
                if (ContainsForbiddenWord(currentRemaining)) return true;
            }
        }
        return false;
    }

    // Sets up the board and spawns the random letter tiles for the player to drag.
    public void LoadRound(string word, Transform ingredient, bool disableRotation, bool dropInCenter = false, bool dropSequentially = false)
    {
        currentWord = word.ToUpper();
        currentIngredient = ingredient;
        currentDisableRotation = disableRotation;
        currentDropInCenter = dropInCenter;
        currentDropSequentially = dropSequentially;

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

        // --- AUTO-SCALE TARGET WORD TO FIT BOARD ---
        CandyBitmapTextUGUI targetBmpText = wordObj.GetComponentInChildren<CandyBitmapTextUGUI>();
        float actualWordWidth = 0f;
        if (targetBmpText != null)
        {
            RectTransform textRt = targetBmpText.GetComponent<RectTransform>();
            if (textRt != null)
            {
                actualWordWidth = textRt.sizeDelta.x;
            }
        }

        RectTransform targetRect = targetBoardContainer.GetComponent<RectTransform>();
        float targetMaxWidth = targetRect != null && targetRect.rect.width > 0 ? targetRect.rect.width : 1100f;
        if (actualWordWidth > targetMaxWidth && targetMaxWidth > 0)
        {
            float tScale = targetMaxWidth / actualWordWidth;
            targetBoardContainer.localScale = new Vector3(tScale, tScale, 1f);
        }
        else
        {
            targetBoardContainer.localScale = Vector3.one;
        }

        string cleanTargetWord = new string(charactersToSpawn.ToArray());
        int attempts = 0;
        string shuffledString;
        do
        {
            ShuffleList(charactersToSpawn);
            shuffledString = new string(charactersToSpawn.ToArray());
            attempts++;
        }
        while ((shuffledString == cleanTargetWord || WillFormForbiddenWord(shuffledString, cleanTargetWord)) && attempts < 50);

        Transform currentRow = null;
        
        // --- DYNAMIC OPTIMAL LAYOUT FOR MAXIMUM TILE SIZE & TOUCHABILITY ---
        float tileWidth = 180f;
        if (WordTile != null)
        {
            UnityEngine.UI.LayoutElement le = WordTile.GetComponent<UnityEngine.UI.LayoutElement>();
            if (le != null && le.preferredWidth > 0) tileWidth = le.preferredWidth;
        }

        float verticalSpacing = 20f;
        UnityEngine.UI.VerticalLayoutGroup vg = letterPoolContainer.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (vg != null) 
        {
            verticalSpacing = vg.spacing;
            vg.childAlignment = TextAnchor.MiddleCenter;
            vg.childControlWidth = true;
            vg.childControlHeight = true;
            vg.childForceExpandWidth = false;
            vg.childForceExpandHeight = false;
        }
        
        RectTransform poolRect = letterPoolContainer.GetComponent<RectTransform>();
        float actualMaxWidth = poolRect != null && poolRect.rect.width > 0 ? poolRect.rect.width : 1165f;
        float actualMaxHeight = poolRect != null && poolRect.rect.height > 0 ? poolRect.rect.height : 419f;

        int N = charactersToSpawn.Count;
        int optimalCols = N;
        float optimalScale = 0f;
        int bestRows = 1;

        // Iterate through all possible column counts to find the arrangement that yields the LARGEST tile scale
        for (int c = 1; c <= N; c++)
        {
            int r = Mathf.CeilToInt((float)N / c);
            float totalW = (c * tileWidth) + ((c - 1) * verticalSpacing);
            float totalH = (r * tileWidth) + ((r - 1) * verticalSpacing);

            float sW = actualMaxWidth / totalW;
            float sH = actualMaxHeight / totalH;
            float fitScale = Mathf.Min(sW, sH);
            float effectiveScale = Mathf.Min(1.0f, fitScale);

            if (effectiveScale > optimalScale + 0.001f)
            {
                optimalScale = effectiveScale;
                optimalCols = c;
                bestRows = r;
            }
            else if (Mathf.Abs(effectiveScale - optimalScale) <= 0.001f)
            {
                // Tie-breaker: if both achieve the same maximum scale, prefer fewer rows
                if (r < bestRows)
                {
                    optimalScale = effectiveScale;
                    optimalCols = c;
                    bestRows = r;
                }
            }
        }

        activeCols = optimalCols;
        letterPoolContainer.localScale = new Vector3(optimalScale, optimalScale, 1f);

        for (int i = 0; i < charactersToSpawn.Count; i++)
        {
            if (i % activeCols == 0)
            {
                GameObject rowObj = new GameObject("Row_" + (i / activeCols), typeof(RectTransform));
                rowObj.layer = letterPoolContainer.gameObject.layer;
                rowObj.transform.SetParent(letterPoolContainer, false);
                rowObj.transform.localScale = Vector3.one;
                rowObj.transform.localPosition = Vector3.zero;

                UnityEngine.UI.LayoutElement rowLe = rowObj.AddComponent<UnityEngine.UI.LayoutElement>();
                rowLe.preferredHeight = tileWidth;
                rowLe.minHeight = tileWidth;
                rowLe.flexibleHeight = 0;

                UnityEngine.UI.HorizontalLayoutGroup hg = rowObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                hg.childAlignment = TextAnchor.MiddleCenter;
                hg.spacing = verticalSpacing;
                hg.childControlWidth = true;
                hg.childControlHeight = true;
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
        PlayCorrectLetterSfx();
        RepackTiles();
    }

    public void OnWordComplete()
    {
        PlayWordCompleteSfx();
        StartCoroutine(HandleWordComplete());
    }

    public void PlayCorrectLetterSfx()
    {
        PlaySfx(correctLetterSfx);
    }

    public void PlayWrongLetterSfx()
    {
        PlaySfx(wrongLetterSfx);
    }

    public void PlayWordCompleteSfx()
    {
        PlaySfx(wordCompleteSfx);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null) sfxAudioSource = gameObject.AddComponent<AudioSource>();
            SetupAudioMixerGroup(sfxAudioSource);
        }
        sfxAudioSource.PlayOneShot(clip);
    }

    private void SetupAudioMixerGroup(AudioSource source)
    {
        if (AudioManager.instance != null && AudioManager.instance.audioMixer != null)
        {
            UnityEngine.Audio.AudioMixerGroup[] groups = AudioManager.instance.audioMixer.FindMatchingGroups("SFX");
            if (groups != null && groups.Length > 0)
            {
                source.outputAudioMixerGroup = groups[0];
            }
        }
    }

    // Re-organizes the remaining letter tiles neatly after one is consumed.
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

        int currentCols = activeCols > 0 ? activeCols : maxCols;
        for (int i = 0; i < activeTiles.Count; i++)
        {
            int rowIndex = i / currentCols;
            if (rowIndex < activeRows.Count)
            {
                activeTiles[i].SetParent(activeRows[rowIndex], false);
            }
        }

        // Hide rows that are empty so remaining rows automatically re-center
        for (int r = 0; r < activeRows.Count; r++)
        {
            int nonConsumedCount = 0;
            foreach (Transform tile in activeRows[r])
            {
                DraggableLetterTile dlt = tile.GetComponent<DraggableLetterTile>();
                if (dlt != null && !dlt.isConsumed && tile.gameObject.activeSelf)
                {
                    nonConsumedCount++;
                }
            }
            activeRows[r].gameObject.SetActive(nonConsumedCount > 0);
        }
    }

    private System.Collections.IEnumerator HandleWordComplete()
    {
        yield return new WaitForSeconds(0.45f); // Comfortable pause to admire the completed word & hear SFX
        ProcessRoundCompletion();
    }

    // Triggered when a word is fully spelled. Starts the drop cutscene.
    private void ProcessRoundCompletion()
    {
        if (spellingRounds == null || spellingRounds.Length == 0 || currentSpellingIndex >= spellingRounds.Length)
            return;

        // Hanya fade out huruf-huruf, kotak kayu (Box) tetap aktif dan static
        FadeLetterContainers(0f, 0.25f);

        if (dropCutscene != null && currentIngredient != null && targetDropArea != null)
        {
            StartCoroutine(PlayCutsceneSequence());
        }
        else
        {
            Debug.LogWarning("[WordMatchingManager] DropCutscene, targetDropArea, or ingredient not assigned — skipping cutscene.");
            AdvanceSpellingRound();
        }
    }

    public void EnsureCameraReferences()
    {
        if (letterVirtualCamera == null || panVirtualCamera == null)
        {
            Cinemachine.CinemachineVirtualCamera[] allCameras = Resources.FindObjectsOfTypeAll<Cinemachine.CinemachineVirtualCamera>();
            foreach (var cam in allCameras)
            {
                if (cam == null || !cam.gameObject.scene.IsValid()) continue;
                if (cam.gameObject.name == "VCam_Letter" && letterVirtualCamera == null)
                {
                    letterVirtualCamera = cam;
                }
                else if (cam.gameObject.name == "VCam_Pan" && panVirtualCamera == null)
                {
                    panVirtualCamera = cam;
                }
            }
        }
    }

    public void EnsureSpawnPointReference()
    {
        if (ingredientSpawnPoint == null)
        {
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t != null && t.gameObject.scene.IsValid() && t.name == "IngredientSpawnPoint")
                {
                    ingredientSpawnPoint = t;
                    break;
                }
            }
        }
    }

    public void EnsureDropAreaReference()
    {
        if (targetDropArea == null)
        {
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t != null && t.gameObject.scene.IsValid() && t.name == "DropArea")
                {
                    targetDropArea = t;
                    break;
                }
            }
        }
        if (panContentContainer == null && targetDropArea != null)
        {
            panContentContainer = targetDropArea;
        }
    }

    public void SyncCameraBlendWithDropDuration(bool isSequential = false)
    {
        if (dropCutscene == null) return;
        float targetDuration = isSequential ? dropCutscene.SequentialPieceDuration : dropCutscene.DropDuration;

        if (Camera.main != null)
        {
            Cinemachine.CinemachineBrain brain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
            if (brain != null && brain.m_CustomBlends != null && brain.m_CustomBlends.m_CustomBlends != null)
            {
                for (int i = 0; i < brain.m_CustomBlends.m_CustomBlends.Length; i++)
                {
                    if (brain.m_CustomBlends.m_CustomBlends[i].m_From == "VCam_Letter" &&
                        brain.m_CustomBlends.m_CustomBlends[i].m_To == "VCam_Pan")
                    {
                        var blend = brain.m_CustomBlends.m_CustomBlends[i];
                        blend.m_Blend.m_Style = Cinemachine.CinemachineBlendDefinition.Style.EaseInOut;
                        blend.m_Blend.m_Time = targetDuration;
                        brain.m_CustomBlends.m_CustomBlends[i] = blend;
                        break;
                    }
                }
            }
        }
    }

    private System.Collections.IEnumerator PlayCutsceneSequence()
    {
        EnsureCameraReferences();
        EnsureSpawnPointReference();
        EnsureDropAreaReference();

        bool isLastRound = (currentSpellingIndex >= spellingRounds.Length - 1);

        // Pastikan di awal reveal kamera tetap menghadap ke rak kayu / huruf
        if (letterVirtualCamera != null)
        {
            letterVirtualCamera.gameObject.SetActive(true);
            letterVirtualCamera.Priority = 20;
        }
        if (panVirtualCamera != null)
        {
            panVirtualCamera.Priority = 10;
        }

        // 1. Munculkan (enable) visual bahan makanan tepat di rak kayu (IngredientSpawnPoint)
        if (dropCutscene != null)
        {
            yield return StartCoroutine(dropCutscene.RevealAtSpawnPoint(currentIngredient, ingredientSpawnPoint));
            if (dropCutscene.delayBeforeDrop > 0f)
            {
                yield return new WaitForSeconds(dropCutscene.delayBeforeDrop);
            }
        }
        else if (currentIngredient != null)
        {
            if (ingredientSpawnPoint != null)
            {
                Vector3 p = ingredientSpawnPoint.position;
                p.z = 0f;
                currentIngredient.position = p;
            }
            currentIngredient.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.35f);
        }

        // 2. Persiapkan fungsi pergerakan kamera ke wajan (hanya dipicu saat potongan terakhir dijatuhkan)
        bool cameraTriggered = false;
        void TriggerPanCamera()
        {
            if (cameraTriggered) return;
            cameraTriggered = true;

            onDropCutsceneStart?.Invoke();
            EnsureCameraReferences();
            SyncCameraBlendWithDropDuration(currentDropSequentially);
            if (panVirtualCamera != null)
            {
                panVirtualCamera.gameObject.SetActive(true);
                panVirtualCamera.Priority = 25; // Cinemachine blend ke wajan
            }
        }

        bool dropFinished = false;
        if (dropCutscene != null)
        {
            dropCutscene.DropToPan(currentIngredient, ingredientSpawnPoint, targetDropArea, panContentContainer, currentDisableRotation, currentDropInCenter, currentDropSequentially, () =>
            {
                dropFinished = true;
            }, TriggerPanCamera);
        }
        else
        {
            TriggerPanCamera();
            dropFinished = true;
        }

        // Tunggu hingga drop selesai
        while (!dropFinished)
        {
            yield return null;
        }

        // Safeguard: Pastikan kamera beralih ke wajan jika belum sempat terpicu
        TriggerPanCamera();

        // Tunggu hingga camera blend selesai jika masih dalam proses blend
        yield return StartCoroutine(WaitForCameraBlend());

        // 3. Cek apakah ini kata terakhir atau masih ada ronde berikutnya
        if (isLastRound)
        {
            // Selesai seluruh round: kamera tetap di wajan untuk fase Stir (Mengaduk)!
            yield return new WaitForSeconds(postDropWaitDelay);

            // Sembunyikan huruf-huruf secara permanen, biarkan kotak kayu (Box) tetap static di rak
            SetLetterContainersVisible(false);

            Debug.Log("[WordMatchingManager] Seluruh kata selesai dieja! Beralih langsung ke fase Stir (Mengaduk)...");
            onSpellingComplete?.Invoke();
        }
        else
        {
            // Masih ada kata berikutnya: kembali ke rak huruf
            AdvanceSpellingRound();
        }
    }

    private System.Collections.IEnumerator WaitForCameraBlend()
    {
        if (Camera.main != null)
        {
            Cinemachine.CinemachineBrain brain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
            if (brain != null)
            {
                // Wait 2 frames so Cinemachine LateUpdate has started the blend
                yield return null; 
                yield return null; 
                
                float safetyTimeout = 4f;
                while (brain.IsBlending && safetyTimeout > 0f)
                {
                    safetyTimeout -= Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(1.5f);
            }
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }
    }

    private void AdvanceSpellingRound()
    {
        currentSpellingIndex++;
        StartCoroutine(ResetCameraAndNextRound());
    }

    // Waits for the camera to return to the board, then starts the next round.
    private System.Collections.IEnumerator ResetCameraAndNextRound()
    {
        yield return new WaitForSeconds(postDropWaitDelay);

        if (currentSpellingIndex < spellingRounds.Length)
        {
            EnsureCameraReferences();

            // Lower Pan camera priority and deactivate to smoothly blend back to letter camera
            if (panVirtualCamera != null)
            {
                panVirtualCamera.Priority = 10;
                panVirtualCamera.gameObject.SetActive(false);
            }
            if (letterVirtualCamera != null)
            {
                letterVirtualCamera.gameObject.SetActive(true);
                letterVirtualCamera.Priority = 15;
            }

            onDropCutsceneEnd?.Invoke();

            yield return StartCoroutine(WaitForCameraBlend());

            if (cameraReturnDelay > 0f)
            {
                yield return new WaitForSeconds(cameraReturnDelay); 
            }

            // Tampilkan kembali huruf-huruf baru di dalam kotak kayu yang tetap static
            SetLetterContainersVisible(true);
            CanvasGroup cgTarget = GetOrAddCanvasGroup(targetBoardContainer);
            CanvasGroup cgPool = GetOrAddCanvasGroup(letterPoolContainer);
            if (cgTarget != null) cgTarget.alpha = 1f;
            if (cgPool != null) cgPool.alpha = 1f;

            LoadCurrentSpellingRound();
        }
        else
        {
            onSpellingComplete?.Invoke();
        }
    }

    public void SetLetterContainersVisible(bool visible)
    {
        if (targetBoardContainer != null) targetBoardContainer.gameObject.SetActive(visible);
        if (letterPoolContainer != null) letterPoolContainer.gameObject.SetActive(visible);
    }

    private void FadeLetterContainers(float targetAlpha, float duration, System.Action onComplete = null)
    {
        StartCoroutine(FadeContainersRoutine(targetAlpha, duration, onComplete));
    }

    private System.Collections.IEnumerator FadeContainersRoutine(float targetAlpha, float duration, System.Action onComplete)
    {
        CanvasGroup cgTarget = GetOrAddCanvasGroup(targetBoardContainer);
        CanvasGroup cgPool = GetOrAddCanvasGroup(letterPoolContainer);

        float startTargetAlpha = cgTarget != null ? cgTarget.alpha : 1f;
        float startPoolAlpha = cgPool != null ? cgPool.alpha : 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (cgTarget != null) cgTarget.alpha = Mathf.Lerp(startTargetAlpha, targetAlpha, t);
            if (cgPool != null) cgPool.alpha = Mathf.Lerp(startPoolAlpha, targetAlpha, t);

            yield return null;
        }

        if (cgTarget != null) cgTarget.alpha = targetAlpha;
        if (cgPool != null) cgPool.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            SetLetterContainersVisible(false);
        }

        onComplete?.Invoke();
    }

    private CanvasGroup GetOrAddCanvasGroup(Transform t)
    {
        if (t == null) return null;
        CanvasGroup cg = t.GetComponent<CanvasGroup>();
        if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
        return cg;
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