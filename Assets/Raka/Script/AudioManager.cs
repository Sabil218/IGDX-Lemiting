using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("BGM")]
    public AudioClip mainMenuBGM;
    public AudioClip levelBGM;
    public AudioClip miniGameBGM;

    private AudioSource musicSource;

    private void Awake()
    {
        // Mencegah AudioManager menjadi lebih dari satu
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
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

    public void PlayBGM()
    {
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

        // Kalau musiknya sama, jangan mulai dari awal lagi
        if (musicSource.clip == newClip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopBGM()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();

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

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();

        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}