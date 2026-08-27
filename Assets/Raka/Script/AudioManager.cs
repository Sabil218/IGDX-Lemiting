using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider musicSlider;
    public Slider sfxSlider;

    public Slider pauseMusicSlider;
    public Slider pauseSFXSlider;

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;

        pauseMusicSlider.value = volume;

        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("MusicVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
        }
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;

        pauseSFXSlider.value = volume;

        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
        }
    }

    public void SetPauseMusicVolume()
    {
        float volume = pauseMusicSlider.value;

        musicSlider.value = volume;

        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("MusicVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
        }
    }

    public void SetPauseSFXVolume()
    {
        float volume = pauseSFXSlider.value;

        sfxSlider.value = volume;

        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
        }
    }
}