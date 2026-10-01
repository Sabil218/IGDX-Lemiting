using UnityEngine;

public class CookingNextTrigger : MonoBehaviour
{
    public GameObject descriptionPanel;
    public GameObject nextButton;

    private bool triggered;

    private void Update()
    {
        if (triggered)
            return;

        if (descriptionPanel != null && descriptionPanel.activeSelf)
        {
            triggered = true;

            if (nextButton != null)
            {
                nextButton.SetActive(true);
            }
        }
    }
}