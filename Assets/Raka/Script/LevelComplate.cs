using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    [Header("Level Number")]
    public int levelNumber = 1;

    public void CompleteLevel()
    {
        PlayerPrefs.SetInt(
            "Level" + levelNumber + "Completed",
            1
        );

        PlayerPrefs.Save();

        Debug.Log("Level " + levelNumber + " selesai!");
    }
}