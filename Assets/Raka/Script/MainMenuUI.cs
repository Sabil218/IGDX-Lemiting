using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject optionPanel;
    public GameObject levelSelectPanel;
    public GameObject cutsceneManager;
    public GameObject creditPanel;

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

    private Vector2 levelSelectOriginalPosition;
    private Vector2 almanacOriginalPosition;

    private void Start()
    {
        // Simpan posisi awal tab
        if (levelSelectTab != null)
            levelSelectOriginalPosition = levelSelectTab.anchoredPosition;

        if (almanacTab != null)
            almanacOriginalPosition = almanacTab.anchoredPosition;

        // Sembunyikan panel saat Mainmenu dibuka
        if (optionPanel != null)
            optionPanel.SetActive(false);

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);

        if (creditPanel != null)
            creditPanel.SetActive(false);

        // Atur tab awal ke Level Select
        SetLevelSelectActive();

        // Buka Level Select jika kembali dari gameplay
        if (PlayerPrefs.GetInt("OpenLevelSelect", 0) == 1)
        {
            levelSelectPanel.SetActive(true);

            PlayerPrefs.SetInt("OpenLevelSelect", 0);
            PlayerPrefs.Save();

            SetLevelSelectActive();
        }
    }

    // =========================
    // OPTION
    // =========================

    public void OpenOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    // =========================
    // CREDIT
    // =========================

    public void OpenCredit()
    {
        if (creditPanel != null)
            creditPanel.SetActive(true);
    }

    public void CloseCredit()
    {
        if (creditPanel != null)
            creditPanel.SetActive(false);
    }

    // =========================
    // LEVEL SELECT
    // =========================

    public void OpenLevelSelect()
    {
        if (levelSelectPanel == null)
            return;

        // Sembunyikan panel sementara
        levelSelectPanel.SetActive(false);

        if (cutsceneManager != null)
        {
            CutsceneManager manager =
                cutsceneManager.GetComponent<CutsceneManager>();

            if (manager != null)
            {
                // CutsceneManager menentukan apakah
                // cutscene perlu diputar atau dilewati
                manager.PlayCutscene();
            }
            else
            {
                Debug.LogError(
                    "CutsceneManager component tidak ditemukan!"
                );
            }
        }
        else
        {
            Debug.LogError("Cutscene Manager belum diisi!");
        }
    }

    public void CloseLevelSelect()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    // =========================
    // ALMANAC
    // =========================

    public void OpenAlmanac()
    {
        SetAlmanacActive();
    }

    public void GoToLevelSelect()
    {
        SetLevelSelectActive();
    }

    // =========================
    // TAB LEVEL SELECT
    // =========================

    public void SetLevelSelectActive()
    {
        if (levelSelectPage != null)
            levelSelectPage.SetActive(true);

        if (almanacPanel != null)
            almanacPanel.SetActive(false);

        // Atur posisi dan warna tab Level Select
        if (levelSelectTab != null)
            levelSelectTab.anchoredPosition =
                levelSelectOriginalPosition;

        if (levelSelectTabImage != null)
            levelSelectTabImage.color = activeTabColor;

        // Atur posisi dan warna tab Almanac
        if (almanacTab != null)
            almanacTab.anchoredPosition =
                almanacOriginalPosition +
                new Vector2(inactiveTabX, 0);

        if (almanacTabImage != null)
            almanacTabImage.color = inactiveTabColor;
    }

    // =========================
    // TAB ALMANAC
    // =========================

    public void SetAlmanacActive()
    {
        if (levelSelectPage != null)
            levelSelectPage.SetActive(false);

        if (almanacPanel != null)
            almanacPanel.SetActive(true);

        // Atur posisi dan warna tab Level Select
        if (levelSelectTab != null)
            levelSelectTab.anchoredPosition =
                levelSelectOriginalPosition +
                new Vector2(inactiveTabX, 0);

        if (levelSelectTabImage != null)
            levelSelectTabImage.color = inactiveTabColor;

        // Atur posisi dan warna tab Almanac
        if (almanacTab != null)
            almanacTab.anchoredPosition =
                almanacOriginalPosition;

        if (almanacTabImage != null)
            almanacTabImage.color = activeTabColor;
    }
}