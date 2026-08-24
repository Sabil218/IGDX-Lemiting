using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public GameObject optionPanel;
    public GameObject levelSelectPanel;
    


    private void Start()
    {
        if (PlayerPrefs.GetInt("OpenLevelSelect", 0) == 1)
        {
            levelSelectPanel.SetActive(true);

            PlayerPrefs.SetInt("OpenLevelSelect", 0);
        }
    }
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