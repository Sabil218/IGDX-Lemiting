using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(IngredientTransitionManager))]
public class SlicingManager : MonoBehaviour, ICookingPhase
{
    public static SlicingManager instance { get; private set; }

    [Header("Slicing References")]
    [Tooltip("Daftar bahan yang akan dipotong secara berurutan")]
    [SerializeField] private GameObject[] bahanSlicing;
    
    [Tooltip("Titik kemunculan bahan yang akan dipotong. Jika kosong, akan menggunakan posisi objek ini.")]
    [SerializeField] private Transform spawnPoint;
    
    private int currentSlicingIndex = 0;
    
    //Prefab reference for cleanup
    private GameObject currentSpawnedBahan;

    private IngredientTransitionManager transitionManager;

    [Header("Events")]
    [Tooltip("Event ini akan dipanggil saat seluruh bahan selesai dipotong.")]
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
        StartCoroutine(JedaGantiBahanSlicing());
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
