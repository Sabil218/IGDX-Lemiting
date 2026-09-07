using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadLevel01()
    {
        SceneManager.LoadScene("Level1");
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
}