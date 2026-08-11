using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public GameObject optionPanel;

    public void OpenOption()
    {
        optionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        optionPanel.SetActive(false);
    }
}