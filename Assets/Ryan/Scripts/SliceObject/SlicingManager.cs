using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(IngredientTransitionManager))]
public class SlicingManager : MonoBehaviour, ICookingPhase
{
    public static SlicingManager instance { get; private set; }

    [Header("Slicing References")]
    [SerializeField] private GameObject[] bahanSlicing;
    [SerializeField] private Transform spawnPoint;
    
    private int currentSlicingIndex = 0;
    
    //Prefab reference for cleanup
    private GameObject currentSpawnedBahan;

    private IngredientTransitionManager transitionManager;

    [Header("Audio / SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip[] sliceSfxClips;
    [SerializeField] private AudioClip ingredientCompleteSfx;

    [Header("Events")]
    [SerializeField] private UnityEvent onSlicingComplete;

    //Initialize Singleton
    private void Awake()
    {
        instance = this;
        transitionManager = GetComponent<IngredientTransitionManager>();
    }

    //Start Slicing Phase
    public void StartPhase()
    {
        if (currentSpawnedBahan != null && currentSpawnedBahan.scene.IsValid()) return; 

        currentSlicingIndex = 0;
        
        foreach (GameObject bahan in bahanSlicing)
        {
            if (bahan != null && bahan.scene.IsValid()) bahan.SetActive(false);
        }

        SpawnAndTransitionNext(null);
    }

    private void SpawnAndTransitionNext(GameObject oldIngredient)
    {
        if (currentSlicingIndex < bahanSlicing.Length)
        {
            GameObject bahanAktif = bahanSlicing[currentSlicingIndex];
            GameObject newIngredientObj = null;

            if (bahanAktif != null)
            {
                Transform targetSpawn = spawnPoint != null ? spawnPoint : transform;
                
                if (!bahanAktif.scene.IsValid())
                {
                    newIngredientObj = Instantiate(bahanAktif, targetSpawn.position, targetSpawn.rotation, transform);
                }
                else
                {
                    newIngredientObj = bahanAktif;
                }
                
                currentSpawnedBahan = newIngredientObj;

                transitionManager.TransitionToNextIngredient(oldIngredient, newIngredientObj, targetSpawn.position);
            }
        }
    }

    //Ingredient Slice Complete Handler
    public void BahanSelesaiDipotong()
    {
        PlayIngredientCompleteSfx();
        StartCoroutine(JedaGantiBahanSlicing());
    }

    public void PlaySliceSfx(AudioClip customClip = null)
    {
        AudioClip clipToPlay = customClip;
        if (clipToPlay == null && sliceSfxClips != null && sliceSfxClips.Length > 0)
        {
            clipToPlay = sliceSfxClips[Random.Range(0, sliceSfxClips.Length)];
        }

        if (clipToPlay != null)
        {
            PlaySfx(clipToPlay);
        }
    }

    public void PlayIngredientCompleteSfx()
    {
        if (ingredientCompleteSfx != null)
        {
            PlaySfx(ingredientCompleteSfx);
        }
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

    //Delay before next ingredient
    private IEnumerator JedaGantiBahanSlicing()
    {
        yield return new WaitForSeconds(0.35f);
        
        GameObject oldIngredient = currentSpawnedBahan;
        currentSlicingIndex++;

        if (currentSlicingIndex < bahanSlicing.Length)
        {
            SpawnAndTransitionNext(oldIngredient);
        }
        else
        {
            // Transition out the last ingredient before completing the phase
            Transform targetSpawn = spawnPoint != null ? spawnPoint : transform;
            transitionManager.TransitionToNextIngredient(oldIngredient, null, targetSpawn.position, () => {
                onSlicingComplete?.Invoke();
            });
        }
    }
}
