using UnityEngine;
using UnityEngine.UI;

public class LevelUnlockManager : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button[] levelButtons;

    [Header("Opacity")]
    [Range(0f, 1f)]
    public float lockedAlpha = 0.4f;

    [Range(0f, 1f)]
    public float unlockedAlpha = 1f;

    [Header("Testing")]
    public bool resetProgressOnStart = false;

    private void Start()
    {
        if (resetProgressOnStart)
        {
            ResetProgress();
        }

        UpdateLevelButtons();
    }

    public void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;

            bool unlocked;

            if (levelNumber == 1)
            {
                unlocked = true;
            }
            else
            {
                unlocked = PlayerPrefs.GetInt(
                    "Level" + (levelNumber - 1) + "Completed",
                    0
                ) == 1;
            }

            SetButtonState(levelButtons[i], unlocked);
        }
    }

    private void SetButtonState(Button button, bool unlocked)
    {
        if (button == null)
            return;

        button.interactable = unlocked;

        CanvasGroup canvasGroup =
            button.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                button.gameObject.AddComponent<CanvasGroup>();
        }

        if (unlocked)
        {
            canvasGroup.alpha = unlockedAlpha;
        }
        else
        {
            canvasGroup.alpha = lockedAlpha;
        }
    }

    public void CompleteLevel(int levelNumber)
    {
        PlayerPrefs.SetInt(
            "Level" + levelNumber + "Completed",
            1
        );

        PlayerPrefs.Save();

        UpdateLevelButtons();
    }

    public void ResetProgress()
    {
        for (int i = 1; i <= levelButtons.Length; i++)
        {
            PlayerPrefs.DeleteKey(
                "Level" + i + "Completed"
            );
        }

        PlayerPrefs.Save();
    }
}