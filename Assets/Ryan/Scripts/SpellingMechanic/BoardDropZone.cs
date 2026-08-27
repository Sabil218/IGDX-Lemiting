using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Approach -> Image used to enabling raycast
public class BoardDropZone : MonoBehaviour, IDropHandler
{
    [Header("Visual")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private UnityEngine.UI.Image backgroundImage;

    [Header("State Colors")]
    [Tooltip("Warna sebelum selesai")]
    [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.5f); //Unanswered Letter
    [Tooltip("Warna Setelah selesai")]
    [SerializeField] private Color completedColor = new Color(0f, 1f, 0.2f, 1f); //Answered Correct Letter
    
    private string fullWord;
    private int completedLength = 0;

    //Setup target word and visual
    public void InitializeWord(string word)
    {
        fullWord = word.ToUpper();
        completedLength = 0;

//Making Transparent Image
        if (backgroundImage != null) 
        {
            Color c = backgroundImage.color;
            c.a = 0f;
            backgroundImage.color = c;
            backgroundImage.enabled = true;
        }

        SkipSpacesIfAny();
        UpdateDisplay();
    }

    //Override current progress
    public void SetCompleted(int newCompletedLength)
    {
        completedLength = newCompletedLength;
        SkipSpacesIfAny();
        UpdateDisplay();
    }

    //Auto-skip space characters
    private void SkipSpacesIfAny()
    {
        if (string.IsNullOrEmpty(fullWord)) return;
        
        while (completedLength < fullWord.Length && fullWord[completedLength] == ' ')
        {
            completedLength++;
        }
    }

    //Refresh text UI with colors
    private void UpdateDisplay()
    {
        if (displayText == null) return;

        string richText = "";
        string hexLocked = ColorUtility.ToHtmlStringRGBA(lockedColor);
        string hexCompleted = ColorUtility.ToHtmlStringRGBA(completedColor);

        for (int i = 0; i < fullWord.Length; i++)
        {
            if (fullWord[i] == ' ')
            {
                richText += " ";
            }
            else if (i < completedLength)
            {
                richText += $"<color=#{hexCompleted}>{fullWord[i]}</color>";
            }
            else
            {
                richText += $"<color=#{hexLocked}>{fullWord[i]}</color>";
            }
        }

        displayText.text = richText;
    }

    //Handle tile drop event
    public void OnDrop(PointerEventData eventData)
    {
        DraggableLetterTile tile = eventData.pointerDrag?.GetComponent<DraggableLetterTile>();
        
        if (tile != null)
        {
            TrySpellLetter(tile);
        }
    }

//Check Word and Answer
    private void TrySpellLetter(DraggableLetterTile tile)
    {
        if (completedLength >= fullWord.Length) return;

        char expectedChar = fullWord[completedLength];

        if (tile.Letter == expectedChar)
        {
            tile.Consume();
            
            completedLength++;
            SkipSpacesIfAny();
            
            UpdateDisplay();

            if (WordMatchingManager.instance != null)
            {
                WordMatchingManager.instance.OnLetterCorrectlySpelled();
            }

            if (completedLength >= fullWord.Length)
            {
                Debug.Log("Phase Complete");
                if (WordMatchingManager.instance != null)
                {
                    WordMatchingManager.instance.OnWordComplete();
                }
            }
        }
        else
        {
            Debug.Log($"Salah!!!, Seharusnya{expectedChar},  bukan {tile.Letter}");
        }
    }
}