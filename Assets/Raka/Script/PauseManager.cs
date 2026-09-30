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
        Time.timeScale = 0f;

        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        pausePanel.SetActive(false);
    }

    public void ChangeMusicVolume()
    {
        if (AudioManager.instance != null && musicSlider != null)
        {
            AudioManager.instance.SetMusicVolume(musicSlider.value);
        }
    }

    public void ChangeSFXVolume()
    {
        if (AudioManager.instance != null && sfxSlider != null)
        {
            AudioManager.instance.SetSFXVolume(sfxSlider.value);
        }
    }

    public void GoHome()
    {
        Time.timeScale = 1f;

        if (LoadingManager.instance != null)
        {
            LoadingManager.instance.LoadScene("Mainmenu");
        }
    }
}