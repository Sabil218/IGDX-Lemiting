using UnityEngine;

public class CookingNextTrigger : MonoBehaviour
{
    public GameObject descriptionPanel;
    public GameObject nextButton;

    private bool wasPanelActive;
    private bool triggered;

    private void Start()
    {
        if (descriptionPanel != null)
        {
            wasPanelActive = descriptionPanel.activeSelf;
        }

        if (nextButton != null)
        {
            nextButton.SetActive(false);
        }
    }

    private void Update()
    {
        if (triggered || descriptionPanel == null)
            return;

        bool panelActive = descriptionPanel.activeSelf;

        if (!wasPanelActive && panelActive)
        {
            triggered = true;

            if (nextButton != null)
            {
                nextButton.SetActive(true);
            }
        }

        wasPanelActive = panelActive;
    }
}