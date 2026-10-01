using UnityEngine;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject cutsceneImage;
    public GameObject levelSelectPanel;
    public AudioManager audioManager;

    private const string CutsceneKey = "LevelSelectCutscenePlayed";

    private void Start()
    {
        cutsceneImage.SetActive(false);

        videoPlayer.loopPointReached -= VideoFinished;
        videoPlayer.loopPointReached += VideoFinished;
    }

    public void PlayCutscene()
    {
        // Periksa apakah cutscene sudah pernah diselesaikan
        if (PlayerPrefs.GetInt(CutsceneKey, 0) == 1)
        {
            levelSelectPanel.SetActive(true);
            return;
        }

        // Sembunyikan Level Select
        levelSelectPanel.SetActive(false);

        // Matikan BGM
        if (audioManager != null)
        {
            audioManager.StopBGM();
        }

        // Tampilkan dan putar cutscene
        cutsceneImage.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void VideoFinished(VideoPlayer vp)
    {
        // Tandai cutscene sudah selesai
        PlayerPrefs.SetInt(CutsceneKey, 1);
        PlayerPrefs.Save();

        // Sembunyikan cutscene
        cutsceneImage.SetActive(false);

        // Tampilkan Level Select
        levelSelectPanel.SetActive(true);

        // Nyalakan kembali BGM
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
        }
    }

    // Untuk mengulang pengujian di Unity Editor
    [ContextMenu("Reset Cutscene Status")]
    private void ResetCutsceneStatus()
    {
        PlayerPrefs.DeleteKey(CutsceneKey);
        PlayerPrefs.Save();

        Debug.Log("Status cutscene berhasil di-reset.");
    }
}