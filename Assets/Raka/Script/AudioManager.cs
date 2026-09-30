
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer audioMixer;

    [Header("BGM")]
    public AudioClip mainMenuBGM;
    public AudioClip levelBGM;
    public AudioClip miniGameBGM;

    private AudioSource musicSource;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();

        // Terapkan volume yang tersimpan
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        ApplyMusicVolume(musicVolume);
        ApplySFXVolume(sfxVolume);
    }

    private void Start()
    {
        PlayBGM();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGM();
    }

    // =========================================
    // BGM
    // =========================================

    public void PlayBGM()
    {
        if (musicSource == null)
            return;

        string sceneName = SceneManager.GetActiveScene().name;

        AudioClip newClip;

        if (sceneName == "Mainmenu")
        {
            newClip = mainMenuBGM;
        }
        else if (sceneName.Contains("Mini"))
        {
            newClip = miniGameBGM;
        }
        else
        {
            newClip = levelBGM;
        }

        if (musicSource.clip == newClip && musicSource.isPlaying)
            return;

        if (newClip == null)
        {
            musicSource.Stop();
            return;
        }

        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopBGM()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    // =========================================
    // MUSIC VOLUME
    // =========================================

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();

        ApplyMusicVolume(volume);
    }

    private void ApplyMusicVolume(float volume)
    {
        if (audioMixer == null)
            return;

        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("MusicVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat(
                "MusicVolume",
                Mathf.Log10(volume) * 20f
            );
        }
    }

    // =========================================
    // SFX VOLUME
    // =========================================

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();

        ApplySFXVolume(volume);
    }

    private void ApplySFXVolume(float volume)
    {
        if (audioMixer == null)
            return;

        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat(
                "SFXVolume",
                Mathf.Log10(volume) * 20f
            );
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }
}