using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    [Header("Loading Screen")]
    public GameObject loadingPanel;
    public Slider loadingBar;

    [Header("Loading Settings")]
    public float loadingDuration = 1f;

    private bool isLoading = false;

    private void Awake()
    {
        // Mencegah LoadingManager menjadi dua
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Tetap ada saat pindah scene
        DontDestroyOnLoad(gameObject);

        // Sembunyikan loading panel saat game mulai
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    // Dipakai untuk pindah scene dengan loading screen
    public void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Nama scene belum diisi!");
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        // Tampilkan loading screen
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Reset loading bar
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }

        // Mulai loading scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Jangan langsung masuk ke scene baru
        operation.allowSceneActivation = false;

        float timer = 0f;

        // Animasi loading bar minimal sesuai durasi
        while (timer < loadingDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / loadingDuration);

            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }

            yield return null;
        }

        // Tunggu sampai scene sudah siap
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        // Penuh
        if (loadingBar != null)
        {
            loadingBar.value = 1f;
        }

        // Sedikit jeda supaya loading 100% terlihat
        yield return new WaitForSecondsRealtime(0.2f);

        // Masuk ke scene baru
        operation.allowSceneActivation = true;

        yield return null;

        // Sembunyikan loading screen setelah scene baru aktif
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        isLoading = false;
    }

    // Kembali ke Main Menu
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        LoadScene("Mainmenu");
    }

    // Kembali ke Main Menu dan langsung membuka Level Select
    public void LoadLevelSelect()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("OpenLevelSelect", 1);
        PlayerPrefs.Save();

        LoadScene("Mainmenu");
    }

    // Mengulang level yang sedang dimainkan
    public void RetryLevel()
    {
        Time.timeScale = 1f;

        LoadScene(SceneManager.GetActiveScene().name);
    }
}