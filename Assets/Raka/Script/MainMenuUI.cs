using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject optionPanel;
    public GameObject levelSelectPanel;

    [Header("Pages")]
    public GameObject levelSelectPage;
    public GameObject almanacPanel;

    [Header("Bookmark Tabs")]
    public RectTransform levelSelectTab;
    public RectTransform almanacTab;

    public Image levelSelectTabImage;
    public Image almanacTabImage;

    [Header("Tab Settings")]
    public float inactiveTabX = -20f;

    public Color activeTabColor = Color.white;
    public Color inactiveTabColor =
        new Color(0.65f, 0.65f, 0.65f, 1f);


    // Menyimpan posisi awal kedua tab
    private Vector2 levelSelectOriginalPosition;
    private Vector2 almanacOriginalPosition;


    private void Start()
    {
        // Simpan posisi asli tab dari Unity
        levelSelectOriginalPosition = levelSelectTab.anchoredPosition;
        almanacOriginalPosition = almanacTab.anchoredPosition;

        // Kondisi awal panel
        optionPanel.SetActive(false);
        levelSelectPanel.SetActive(false);

        // Kondisi awal halaman
        SetLevelSelectActive();


        // Jika kembali dari scene lain
        if (PlayerPrefs.GetInt("OpenLevelSelect", 0) == 1)
        {
            levelSelectPanel.SetActive(true);

            PlayerPrefs.SetInt("OpenLevelSelect", 0);
            PlayerPrefs.Save();

            SetLevelSelectActive();
        }
    }


    // =================================
    // OPTION
    // =================================

    public void OpenOption()
    {
        optionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        optionPanel.SetActive(false);
    }


    // =================================
    // BUKA LEVEL SELECT
    // =================================

    public void OpenLevelSelect()
    {
        levelSelectPanel.SetActive(true);

        // Selalu mulai dari halaman Level Select
        SetLevelSelectActive();
    }

    public void CloseLevelSelect()
    {
        levelSelectPanel.SetActive(false);
    }


    // =================================
    // PINDAH KE ALMANAC
    // =================================

    public void OpenAlmanac()
    {
        SetAlmanacActive();
    }


    // =================================
    // PINDAH KE LEVEL SELECT
    // =================================

    public void GoToLevelSelect()
    {
        SetLevelSelectActive();
    }


    // =================================
    // LEVEL SELECT AKTIF
    // =================================

    public void SetLevelSelectActive()
    {
        // Tampilkan halaman Level Select
        levelSelectPage.SetActive(true);

        // Sembunyikan halaman Almanac
        almanacPanel.SetActive(false);


        // LEVEL SELECT TAB AKTIF
        // Kembali ke posisi asli
        levelSelectTab.anchoredPosition =
            levelSelectOriginalPosition;

        // Warna terang
        levelSelectTabImage.color =
            activeTabColor;


        // ALMANAC TAB TIDAK AKTIF
        // Geser sedikit dari posisi asli
        almanacTab.anchoredPosition =
            almanacOriginalPosition +
            new Vector2(inactiveTabX, 0);

        // Warna lebih gelap
        almanacTabImage.color =
            inactiveTabColor;
    }


    // =================================
    // ALMANAC AKTIF
    // =================================

    public void SetAlmanacActive()
    {
        // Sembunyikan halaman Level Select
        levelSelectPage.SetActive(false);

        // Tampilkan halaman Almanac
        almanacPanel.SetActive(true);


        // LEVEL SELECT TAB TIDAK AKTIF
        // Geser sedikit dari posisi asli
        levelSelectTab.anchoredPosition =
            levelSelectOriginalPosition +
            new Vector2(inactiveTabX, 0);

        // Warna lebih gelap
        levelSelectTabImage.color =
            inactiveTabColor;


        // ALMANAC TAB AKTIF
        // Kembali ke posisi asli
        almanacTab.anchoredPosition =
            almanacOriginalPosition;

        // Warna terang
        almanacTabImage.color =
            activeTabColor;
    }
}