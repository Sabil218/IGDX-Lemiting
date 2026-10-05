using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSkip : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // N = Next Level
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("N ditekan - Skip ke level berikutnya!");
            NextLevel();
        }

        // F1 = Level 1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("F1 ditekan - Membuka Level 1!");
            LoadLevel(1);
        }

        // F2 = Level 2
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("F2 ditekan - Membuka Level 2!");
            LoadLevel(2);
        }

        // F3 = Level 3
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("F3 ditekan - Membuka Level 3!");
            LoadLevel(3);
        }

        // F4 = Level 4
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("F4 ditekan - Membuka Level 4!");
            LoadLevel(4);
        }

        // F5 = Level 5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("F5 ditekan - Membuka Level 5!");
            LoadLevel(5);
        }
    }

    private void NextLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        Debug.Log("Current Scene Index: " + currentScene);
        Debug.Log("Next Scene Index: " + nextScene);

        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning("Tidak ada scene berikutnya!");
        }
    }

    private void LoadLevel(int level)
    {
        Debug.Log("Mencoba membuka Build Index: " + level);

        if (level < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(level);
        }
        else
        {
            Debug.LogError(
                "Build Index " + level +
                " tidak ditemukan! Jumlah scene: " +
                SceneManager.sceneCountInBuildSettings
            );
        }
    }
}