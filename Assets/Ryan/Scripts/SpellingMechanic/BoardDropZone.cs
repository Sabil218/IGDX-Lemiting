using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoardDropZone : MonoBehaviour, IDropHandler
{
    [Header("Visual")]
    [SerializeField] private CandyBitmapTextUGUI displayText;
    [SerializeField] private Image backgroundImage;

    [Header("State Colors")]
    [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color completedColor = new Color(0f, 1f, 0.2f, 1f);

    private string fullWord;
    private int completedLength = 0;

    // Sets up the target word and clears the board visuals.
    public void InitializeWord(string word)
    {
        fullWord = word.ToUpper();
        completedLength = 0;

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

    public void SetCompleted(int newCompletedLength)
    {
        completedLength = newCompletedLength;
        SkipSpacesIfAny();
        UpdateDisplay();
    }

    // Automatically skips over space characters so players don't have to fill them.
    private void SkipSpacesIfAny()
    {
        if (string.IsNullOrEmpty(fullWord)) return;

        while (completedLength < fullWord.Length && fullWord[completedLength] == ' ')
        {
            completedLength++;
        }
    }

    // Refreshes the visual text and colors on the board based on current progress.
    private void UpdateDisplay()
    {
        if (displayText == null) return;

        string finalWord = "";

        for (int i = 0; i < fullWord.Length; i++)
        {
            finalWord += fullWord[i];
        }

        displayText.Text = finalWord;

        Image[] glyphImages = displayText.GetComponentsInChildren<Image>(true);
        int glyphIndex = 0;

        for (int i = 0; i < fullWord.Length; i++)
        {
            if (fullWord[i] == ' ') continue;

            if (glyphIndex < glyphImages.Length)
            {
                if (i < completedLength)
                {
                    glyphImages[glyphIndex].color = completedColor;
                }
                else
                {
                    glyphImages[glyphIndex].color = lockedColor;
                }
                glyphIndex++;
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableLetterTile tile = eventData.pointerDrag?.GetComponent<DraggableLetterTile>();

        if (tile != null)
        {
            TrySpellLetter(tile);
        }
    }

    // Checks if the dropped letter matches the expected next letter in the word.
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
                if (WordMatchingManager.instance != null)
                {
                    WordMatchingManager.instance.OnWordComplete();
                }
            }
        }
        else
        {
            Debug.Log($"Salah!!!, Seharusnya {expectedChar}, bukan {tile.Letter}");
            if (WordMatchingManager.instance != null)
            {
                WordMatchingManager.instance.PlayWrongLetterSfx();
            }
        }
    }
}