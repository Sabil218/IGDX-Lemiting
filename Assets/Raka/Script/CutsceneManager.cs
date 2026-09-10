using UnityEngine;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject cutsceneImage;
    public GameObject levelSelectPanel;
    public AudioManager audioManager;

    // Menyimpan status hanya selama Play Mode
    private static bool cutsceneAlreadyPlayed = false;

    // Memastikan status di-reset ketika mulai Play Mode baru
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        cutsceneAlreadyPlayed = false;
    }

    private void Start()
    {
        cutsceneImage.SetActive(false);

        videoPlayer.loopPointReached += VideoFinished;
    }

    public void PlayCutscene()
    {
        // Kalau sudah pernah dimainkan dalam sesi Play ini,
        // langsung buka Level Select
        if (cutsceneAlreadyPlayed)
        {
            levelSelectPanel.SetActive(true);
            return;
        }

        // Tandai sudah dimainkan
        cutsceneAlreadyPlayed = true;

        // Sembunyikan Level Select
        levelSelectPanel.SetActive(false);

        // Matikan BGM
        if (audioManager != null)
        {
            audioManager.StopBGM();
        }

        // Tampilkan cutscene
        cutsceneImage.SetActive(true);

        // Putar video dari awal
        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void VideoFinished(VideoPlayer vp)
    {
        // Sembunyikan cutscene
        cutsceneImage.SetActive(false);

        // Tampilkan Level Select
        levelSelectPanel.SetActive(true);

        // Nyalakan BGM kembali
        if (audioManager != null)
        {
            audioManager.PlayBGM();
        }
    }
}