using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingButton : MonoBehaviour
{
    // =========================
    // NEXT LEVEL
    // =========================
    public void NextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level1")
        {
            LoadingManager.instance.LoadScene("Level2");
        }
        else if (currentScene == "Level2")
        {
            LoadingManager.instance.LoadScene("Level3");
        }
        else if (currentScene == "Level3")
        {
            LoadingManager.instance.LoadScene("Level4");
        }
        else if (currentScene == "Level4")
        {
            LoadingManager.instance.LoadScene("Level5");
        }
        else if (currentScene == "Level5")
        {
            LoadingManager.instance.LoadScene("Level6");
        }
        else
        {
            Debug.LogWarning(
                "Tidak ada level berikutnya untuk scene: " + currentScene
            );
        }
    }


    // =========================
    // RETRY LEVEL
    // =========================
    public void RetryLevel()
    {
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;

        LoadingManager.instance.LoadScene(currentScene);
    }


    // =========================
    // LEVEL SELECT
    // =========================
    public void LoadLevelSelect()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("OpenLevelSelect", 1);
        PlayerPrefs.Save();

        LoadingManager.instance.LoadScene("Mainmenu");
    }


    // =========================
    // MAIN MENU
    // =========================
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        LoadingManager.instance.LoadScene("Mainmenu");
    }
}