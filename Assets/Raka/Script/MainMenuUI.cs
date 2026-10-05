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

    [Header("Level Select Only Buttons")]
    public GameObject nextPageButton;
    public GameObject otherLevelSelectButton;

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
        Debug.Log("MainMenuUI Start dipanggil.");

        // Simpan posisi awal tab
        if (levelSelectTab != null)
            levelSelectOriginalPosition =
                levelSelectTab.anchoredPosition;

        if (almanacTab != null)
            almanacOriginalPosition =
                almanacTab.anchoredPosition;

        // Sembunyikan panel awal
        if (optionPanel != null)
            optionPanel.SetActive(false);

        if (creditPanel != null)
            creditPanel.SetActive(false);

        // Baca penanda untuk membuka Level Select
        int openLevelSelect =
            PlayerPrefs.GetInt("OpenLevelSelect", 0);

        Debug.Log("OpenLevelSelect saat Mainmenu dibuka: "
            + openLevelSelect);

        // Atur tab awal
        SetLevelSelectActive();

        if (openLevelSelect == 1)
        {
            // Hapus penanda setelah dibaca
            PlayerPrefs.SetInt("OpenLevelSelect", 0);
            PlayerPrefs.Save();

            // Buka Level Select langsung
            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(true);
                Debug.Log("Level Select dibuka dari gameplay.");
            }
            else
            {
                Debug.LogError("Level Select Panel belum diisi!");
            }
        }
        else
        {
            // Masuk Mainmenu secara normal
            if (levelSelectPanel != null)
                levelSelectPanel.SetActive(false);

            Debug.Log("Mainmenu dibuka tanpa Level Select.");
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
        Debug.Log("Tombol Level Select Mainmenu ditekan.");

        if (levelSelectPanel == null)
        {
            Debug.LogError("Level Select Panel belum diisi!");
            return;
        }

        SetLevelSelectActive();

        if (cutsceneManager != null)
        {
            CutsceneManager manager =
                cutsceneManager.GetComponent<CutsceneManager>();

            if (manager != null)
            {
                manager.PlayCutscene();
            }
            else
            {
                Debug.LogError(
                    "Komponen CutsceneManager tidak ditemukan!"
                );

                levelSelectPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning(
                "Cutscene Manager belum diisi. " +
                "Level Select dibuka langsung."
            );

            levelSelectPanel.SetActive(true);
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
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);

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

        // Tampilkan tombol khusus Level Select
        SetLevelSelectButtons(true);

        // Posisi dan warna tab Level Select
        if (levelSelectTab != null)
        {
            levelSelectTab.anchoredPosition =
                levelSelectOriginalPosition;
        }

        if (levelSelectTabImage != null)
            levelSelectTabImage.color = activeTabColor;

        // Posisi dan warna tab Almanac
        if (almanacTab != null)
        {
            almanacTab.anchoredPosition =
                almanacOriginalPosition +
                new Vector2(inactiveTabX, 0);
        }

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

        // Sembunyikan tombol khusus Level Select
        SetLevelSelectButtons(false);

        // Posisi dan warna tab Level Select
        if (levelSelectTab != null)
        {
            levelSelectTab.anchoredPosition =
                levelSelectOriginalPosition +
                new Vector2(inactiveTabX, 0);
        }

        if (levelSelectTabImage != null)
            levelSelectTabImage.color = inactiveTabColor;

        // Posisi dan warna tab Almanac
        if (almanacTab != null)
        {
            almanacTab.anchoredPosition =
                almanacOriginalPosition;
        }

        if (almanacTabImage != null)
            almanacTabImage.color = activeTabColor;
    }

    // =========================
    // LEVEL SELECT BUTTON VISIBILITY
    // =========================

    private void SetLevelSelectButtons(bool visible)
    {
        if (nextPageButton != null)
            nextPageButton.SetActive(visible);

        if (otherLevelSelectButton != null)
            otherLevelSelectButton.SetActive(visible);
    }
}