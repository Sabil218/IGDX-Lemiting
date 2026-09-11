using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SlicingManager : MonoBehaviour, ICookingPhase
{
    public static SlicingManager instance { get; private set; }

    [Header("Slicing References")]
    [Tooltip("Daftar bahan yang akan dipotong secara berurutan")]
    public GameObject[] bahanSlicing;
    
    private int currentSlicingIndex = 0;
    
    //Prefab reference for cleanup
    private GameObject currentSpawnedBahan;

    [Header("Events")]
    [Tooltip("Event ini akan dipanggil saat seluruh bahan selesai dipotong.")]
    public UnityEvent onSlicingComplete;

    //Initialize Singleton
    private void Awake()
    {
        instance = this;
    }

    //Start Slicing Phase
    public void StartPhase()
    {
        currentSlicingIndex = 0;
        TampilkanBahanPotong();
    }

    //Show Current Ingredient
    private void TampilkanBahanPotong()
    {
        foreach (GameObject bahan in bahanSlicing)
        {
            if (bahan != null && bahan.scene.IsValid()) bahan.SetActive(false);
        }

        if (currentSpawnedBahan != null)
        {
            Destroy(currentSpawnedBahan);
            currentSpawnedBahan = null;
        }

        if (currentSlicingIndex < bahanSlicing.Length)
        {
            GameObject bahanAktif = bahanSlicing[currentSlicingIndex];
            if (bahanAktif != null)
            {
                if (!bahanAktif.scene.IsValid())
                {
                    currentSpawnedBahan = Instantiate(bahanAktif, transform.position, transform.rotation, transform);
                    currentSpawnedBahan.SetActive(true);
                }
                else
                {
                    bahanAktif.SetActive(true);
                }
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
        yield return new WaitForSeconds(1f);
        currentSlicingIndex++;

        if (currentSlicingIndex < bahanSlicing.Length)
        {
            TampilkanBahanPotong();
        }
        else
        {

            onSlicingComplete?.Invoke();
        }
    }
}
