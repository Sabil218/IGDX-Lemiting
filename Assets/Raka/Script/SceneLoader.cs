using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    // =========================================
    // SINGLETON
    // =========================================

    public static SceneLoader instance;


    // =========================================
    // LOADING SCREEN
    // =========================================

    [Header("Loading Screen")]
    public GameObject loadingPanel;
    public Slider loadingBar;

    [Header("Loading Settings")]
    public float loadingDuration = 1f;

    private float previousAudioVolume;
    private bool isLoading = false;


    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        // Mencegah SceneLoader menjadi lebih dari satu
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // SceneLoader tetap hidup ketika pindah scene
        DontDestroyOnLoad(gameObject);

        // Loading panel disembunyikan saat awal
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }


    // =========================================
    // LOAD SCENE
    // SEMUA PERPINDAHAN SCENE
    // =========================================

    public void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Nama Scene belum diisi!");
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneName));
    }


    // =========================================
    // ASYNC LOADING
    // =========================================

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        Debug.Log("Loading scene: " + sceneName);


        // =========================================
        // TAMPILKAN LOADING SCREEN
        // =========================================

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }


        // =========================================
        // MATIKAN AUDIO SELAMA LOADING
        // =========================================

        previousAudioVolume = AudioListener.volume;

        AudioListener.volume = 0f;


        // =========================================
        // LOAD SCENE
        // =========================================

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        operation.allowSceneActivation = false;


        // =========================================
        // ANIMASI LOADING BAR
        // =========================================

        float timer = 0f;

        while (timer < loadingDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(timer / loadingDuration);

            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }

            yield return null;
        }


        // =========================================
        // TUNGGU SCENE SELESAI DIMUAT
        // =========================================

        while (operation.progress < 0.9f)
        {
            yield return null;
        }


        // Loading bar penuh
        if (loadingBar != null)
        {
            loadingBar.value = 1f;
        }

        yield return new WaitForSecondsRealtime(0.2f);


        // =========================================
        // KEMBALIKAN AUDIO SEBELUM SCENE AKTIF
        // =========================================

        AudioListener.volume = previousAudioVolume;


        // =========================================
        // AKTIFKAN SCENE BARU
        // =========================================

        operation.allowSceneActivation = true;

        yield return null;


        // =========================================
        // SELESAI
        // =========================================

        isLoading = false;
    }


    // =========================================
    // MAIN MENU
    // =========================================

    public void LoadMainMenu()
    {
        LoadScene("Mainmenu");
    }


    // =========================================
    // LEVEL SELECT
    // =========================================

    public void LoadLevelSelect()
    {
        PlayerPrefs.SetInt("OpenLevelSelect", 1);
        PlayerPrefs.Save();

        LoadScene("Mainmenu");
    }


    // =========================================
    // RETRY LEVEL
    // =========================================

    public void RetryLevel()
    {
        LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}