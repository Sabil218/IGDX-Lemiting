using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    // =========================================
    // LOADING SCREEN
    // =========================================

    [Header("Loading Screen")]
    public GameObject loadingPanel;
    public Slider loadingBar;

    [Header("Loading Settings")]
    public float loadingDuration = 3f;

    private float previousAudioVolume;


    // =========================================
    // LOAD SCENE - UNTUK LEVEL SELECT
    // =========================================
    // Contoh:
    // LoadScene("Level1")
    // LoadScene("Level4")
    // LoadScene("Level5")
    // =========================================

    public void LoadScene(string sceneName)
    {
        Debug.Log("Mencoba membuka scene: " + sceneName);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Nama Scene belum diisi!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }


    // =========================================
    // LOAD SCENE DENGAN LOADING SCREEN
    // =========================================
    // Bisa digunakan untuk Level Select
    // maupun Next Level jika nanti dibutuhkan.
    // =========================================

    public void LoadSceneWithLoading(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Nama Scene belum diisi!");
            return;
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }

        // Simpan volume sebelum loading
        previousAudioVolume = AudioListener.volume;

        // Matikan suara selama loading
        AudioListener.volume = 0f;

        StartCoroutine(LoadSceneAsync(sceneName));
    }


    // =========================================
    // ASYNC LOADING
    // =========================================

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log("Loading scene: " + sceneName);

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        operation.allowSceneActivation = false;

        float timer = 0f;

        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(timer / loadingDuration);

            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }

            yield return null;
        }

        if (loadingBar != null)
        {
            loadingBar.value = 1f;
        }

        // Tunggu sampai scene selesai dimuat
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        // Aktifkan scene baru
        operation.allowSceneActivation = true;

        yield return null;

        // Kembalikan volume
        AudioListener.volume = previousAudioVolume;
    }


    // =========================================
    // MAIN MENU - LOGIC LAMA
    // =========================================

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Mainmenu");
    }


    // =========================================
    // LEVEL SELECT - LOGIC LAMA
    // =========================================
    // Digunakan dari Win Condition.
    // Mainmenu akan otomatis membuka
    // Level Select setelah scene dimuat.
    // =========================================

    public void LoadLevelSelect()
    {
        PlayerPrefs.SetInt("OpenLevelSelect", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Mainmenu");
    }


    // =========================================
    // RETRY LEVEL
    // =========================================

    public void RetryLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}