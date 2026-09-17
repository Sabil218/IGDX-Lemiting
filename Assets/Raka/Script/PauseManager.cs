using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;

    [Header("Pause Audio Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ChangeMusicVolume()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume();
        }
    }

    public void ChangeSFXVolume()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume();
        }
    }

    public void GoHome()
    {
        Time.timeScale = 1f;

        if (SceneLoader.instance != null)
        {
            SceneLoader.instance.LoadMainMenu();
        }
    }
}