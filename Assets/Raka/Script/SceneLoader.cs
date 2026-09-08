using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("Loading Screen")]
    public GameObject loadingPanel;
    public Slider loadingBar;

    [Header("Loading Settings")]
    public float loadingDuration = 5f;

    public void LoadLevel01()
    {
        loadingPanel.SetActive(true);

        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }

        StartCoroutine(LoadLevelAsync("Level1"));
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Mainmenu");
    }

    public void LoadLevelSelect()
    {
        PlayerPrefs.SetInt("OpenLevelSelect", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Mainmenu");
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator LoadLevelAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        operation.allowSceneActivation = false;

        float timer = 0f;

        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / loadingDuration);

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

        // Pastikan scene sudah selesai loading
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        operation.allowSceneActivation = true;
    }
}