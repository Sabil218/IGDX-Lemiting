
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Audio Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void OnEnable()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(
                OnMusicChanged
            );

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(
                OnSFXChanged
            );
    }

    private void Start()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager tidak ditemukan!");
            return;
        }

        float musicVolume =
            PlayerPrefs.GetFloat("MusicVolume", 1f);

        float sfxVolume =
            PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(musicVolume);
            AudioManager.instance.SetMusicVolume(musicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfxVolume);
            AudioManager.instance.SetSFXVolume(sfxVolume);
        }
    }

    private void OnMusicChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMusicVolume(value);
    }

    private void OnSFXChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSFXVolume(value);
    }

    private void OnDisable()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(
                OnMusicChanged
            );

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(
                OnSFXChanged
            );
    }
}