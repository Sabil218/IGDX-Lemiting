
using UnityEngine;
using UnityEngine.Video;
using System;
using System.IO;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene References")]
    public VideoPlayer videoPlayer;
    public GameObject cutsceneImage;
    public GameObject levelSelectPanel;
    public AudioManager audioManager;

    private const string CutsceneKey = "LevelSelectCutscenePlayed";
    private bool isPlaying = false;

    private void Start()
    {
        if (videoPlayer == null || cutsceneImage == null ||
            levelSelectPanel == null)
        {
            Debug.LogError("Referensi CutsceneManager belum lengkap!");
            return;
        }

        cutsceneImage.SetActive(false);
        

        videoPlayer.source = VideoSource.Url;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

#if UNITY_WEBGL && !UNITY_EDITOR
        videoPlayer.url = Application.streamingAssetsPath
            + "/IGDX26_INTRO.mp4";
#else
        string videoPath = Path.Combine(
            Application.streamingAssetsPath,
            "IGDX26_INTRO.mp4"
        );

        videoPlayer.url = new Uri(
            Path.GetFullPath(videoPath)
        ).AbsoluteUri;
#endif

        videoPlayer.loopPointReached -= VideoFinished;
        videoPlayer.loopPointReached += VideoFinished;

        videoPlayer.errorReceived -= VideoError;
        videoPlayer.errorReceived += VideoError;

        Debug.Log("Cutscene URL: " + videoPlayer.url);
    }

    public void PlayCutscene()
    {
        if (videoPlayer == null ||
            cutsceneImage == null ||
            levelSelectPanel == null)
        {
            Debug.LogError("Referensi cutscene belum lengkap!");
            return;
        }

        // Jika cutscene sudah pernah selesai,
        // langsung tampilkan Level Select.
        if (PlayerPrefs.GetInt(CutsceneKey, 0) == 1)
        {
            levelSelectPanel.SetActive(true);
            return;
        }

        // Mencegah cutscene diputar berulang kali.
        if (isPlaying) return;

        isPlaying = true;

        // Sembunyikan Level Select.
        levelSelectPanel.SetActive(false);

        // Hentikan BGM Mainmenu.
        if (audioManager != null)
        {
            audioManager.StopBGM();
        }

        // Tampilkan layar cutscene.
        cutsceneImage.SetActive(true);

        // Siapkan video sebelum diputar.
        videoPlayer.prepareCompleted -= VideoPrepared;
        videoPlayer.prepareCompleted += VideoPrepared;
        videoPlayer.Prepare();
    }

    private void VideoPrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= VideoPrepared;

        if (isPlaying)
        {
            videoPlayer.Play();
            Debug.Log("Cutscene mulai diputar.");
        }
    }

    private void VideoFinished(VideoPlayer vp)
    {
        // Simpan status bahwa cutscene sudah selesai.
        PlayerPrefs.SetInt(CutsceneKey, 1);
        PlayerPrefs.Save();

        isPlaying = false;

        // Sembunyikan cutscene.
        cutsceneImage.SetActive(false);

        // Tampilkan Level Select.
        levelSelectPanel.SetActive(true);

        // Nyalakan kembali BGM.
        if (audioManager != null)
        {
            audioManager.PlayBGM();
        }

        Debug.Log("Cutscene selesai.");
    }

    private void VideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("Cutscene Error: " + message);

        isPlaying = false;

        // Sembunyikan cutscene.
        cutsceneImage.SetActive(false);

        // Tampilkan Level Select.
        levelSelectPanel.SetActive(true);

        // Nyalakan kembali BGM jika terjadi error.
        if (audioManager != null)
        {
            audioManager.PlayBGM();
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= VideoFinished;
            videoPlayer.prepareCompleted -= VideoPrepared;
            videoPlayer.errorReceived -= VideoError;
        }
    }

    [ContextMenu("Reset Cutscene Status")]
    private void ResetCutsceneStatus()
    {
        PlayerPrefs.DeleteKey(CutsceneKey);
        PlayerPrefs.Save();

        Debug.Log("Status cutscene berhasil di-reset.");
    }
}