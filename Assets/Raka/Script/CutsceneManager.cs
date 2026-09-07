using UnityEngine;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject cutsceneImage;
    public GameObject levelSelectPanel;
    public AudioManager audioManager;

    private void Start()
    {
        cutsceneImage.SetActive(false);

        videoPlayer.loopPointReached += VideoFinished;
    }

    public void PlayCutscene()
    {
        // Sembunyikan Level Select
        levelSelectPanel.SetActive(false);

        // Matikan BGM kalau AudioManager tersedia
        if (audioManager != null)
        {
            audioManager.StopBGM();
        }

        // Tampilkan video
        cutsceneImage.SetActive(true);

        // Pastikan video mulai dari awal
        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void VideoFinished(VideoPlayer vp)
    {
        // Sembunyikan video
        cutsceneImage.SetActive(false);

        // Tampilkan Level Select
        levelSelectPanel.SetActive(true);

        // Nyalakan BGM lagi
        if (audioManager != null)
        {
            audioManager.PlayBGM();
        }
    }
}