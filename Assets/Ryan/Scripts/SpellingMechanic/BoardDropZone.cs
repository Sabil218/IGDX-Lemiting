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

    private void SkipSpacesIfAny()
    {
        if (string.IsNullOrEmpty(fullWord)) return;

        while (completedLength < fullWord.Length && fullWord[completedLength] == ' ')
        {
            completedLength++;
        }
    }

    private void UpdateDisplay()
    {
        if (displayText == null) return;

        string finalWord = "";

        // 1. Tampilkan huruf aslinya secara utuh sejak awal, JANGAN diganti jadi setrip ("-")
        for (int i = 0; i < fullWord.Length; i++)
        {
            finalWord += fullWord[i];
        }

        // Masukkan teks utuh ke script Candy
        displayText.Text = finalWord;

        // 2. Warnai setiap gambar huruf secara dinamis
        Image[] glyphImages = displayText.GetComponentsInChildren<Image>(true);
        int glyphIndex = 0;

        for (int i = 0; i < fullWord.Length; i++)
        {
            // Abaikan spasi karena script Candy tidak memproduksi GameObject gambar untuk spasi
            if (fullWord[i] == ' ') continue;

            if (glyphIndex < glyphImages.Length)
            {
                if (i < completedLength)
                {
                    glyphImages[glyphIndex].color = completedColor; // Huruf yang sudah ditebak (Hijau)
                }
                else
                {
                    glyphImages[glyphIndex].color = lockedColor; // Huruf yang belum ditebak (Biru Buram)
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
        }
    }
}