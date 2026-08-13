using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public GameObject optionPanel;
    public GameObject levelSelectPanel;


    public void OpenOption()
    {
        optionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        optionPanel.SetActive(false);
    }

    public void OpenLevelSelect()
    {
        levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        levelSelectPanel.SetActive(false);
    }
}