using UnityEngine;

public class LevelSelectLoader : MonoBehaviour
{
    public void LoadLevelSelect(string sceneName)
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("OpenLevelSelect", 1);
        PlayerPrefs.Save();

        if (LoadingManager.instance != null)
        {
            LoadingManager.instance.LoadScene(sceneName);
        }
    }
}