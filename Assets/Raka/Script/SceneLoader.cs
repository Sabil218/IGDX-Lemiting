using System.Collections;
using System.Collections.Generic;
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
        PlayerPrefs.SetInt("OpenLevelSelect", 1);
        SceneManager.LoadScene("Mainmenu");
    }
}