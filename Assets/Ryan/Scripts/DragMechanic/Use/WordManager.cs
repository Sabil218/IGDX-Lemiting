using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public static WordManager instance { get; private set; }

    [Header("Object References")]
    [SerializeField] Transform RandomArea;
    [SerializeField] Transform TargetArea;
    [SerializeField] GameObject buttonHint;
    [SerializeField] DragHandler letterPrefab;

    [Header("Game Data")]
    [SerializeField] string[] words;

    //State Tracking
    private int currentWordIndex = 0;
    private int wrongAttempts = 0;

    void Start()
    {
        instance = this;
        buttonHint.SetActive(false);
        LoadLevel(currentWordIndex);
    }

    private void LoadLevel(int index)
    {
        wrongAttempts = 0;
        buttonHint.SetActive(false);

        ClearArea(RandomArea);
        ClearArea(TargetArea);

        string currentWord = words[index];
        char[] letters = currentWord.ToCharArray();
        List<char> letterlist = letters.ToList();

        for (int i = 0; i < letters.Length; i++)
        {
            int randomIndex = Random.Range(0, letterlist.Count);
            char randomLetter = letterlist[randomIndex];
            letterlist.RemoveAt(randomIndex);

            DragHandler temp = Instantiate(letterPrefab, RandomArea);
            temp.LetterInit(RandomArea, randomLetter.ToString());
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        yield return new WaitForSeconds(2f);
        currentWordIndex++;

        if (currentWordIndex < words.Length)
        {
            LoadLevel(currentWordIndex);
        }
        else
        {
            Debug.Log("Game Selesai!");
        }
    }

    private void ClearArea(Transform area)
    {
        foreach (Transform child in area)
        {
            Destroy(child.gameObject);
        }
    }
    public void EvaluateBoard()
    {
        string playerAnswer = GetPlayerAnswerFromTargetArea();
        string targetWord = words[currentWordIndex];

        int currentIndex = playerAnswer.Length - 1;

        if (currentIndex < 0) return;

        if (playerAnswer[currentIndex] == targetWord[currentIndex])
        {
            Debug.Log("Huruf benar di posisi: " + currentIndex);
            if (playerAnswer.Length == targetWord.Length)
            {
                Debug.Log("Kata Selesai! Memulai Transisi...");
                LockWinningLetters();
                StartCoroutine(TransitionToNextLevel());
            }
        }
        else
        {
            wrongAttempts++;
            Debug.Log("Huruf Salah! Total Salah: " + wrongAttempts);

            if (wrongAttempts >= 3)
            {
                buttonHint.SetActive(true);
            }

            RejectLastLetter();
        }
    }
    private void RejectLastLetter()
    {
        int lastChildIndex = TargetArea.childCount - 1;
        if (lastChildIndex >= 0)
        {
            TargetArea.GetChild(lastChildIndex).SetParent(RandomArea);
        }
    }
    private string GetPlayerAnswerFromTargetArea()
    {
        string answer = "";
        foreach (Transform child in TargetArea)
        {
            DragHandler letterObject = child.GetComponent<DragHandler>();
            if (letterObject != null)
            {
                answer += letterObject.Letter;
            }
        }
        return answer;
    }
    private void LockWinningLetters()
    {
        foreach (Transform child in TargetArea)
        {
            DragHandler letter = child.GetComponent<DragHandler>();
            if (letter != null)
            {
                letter.enabled = false;
            }
        }
    }
    public string GetCurrentWord()
    {
        return words[currentWordIndex];
    }
    private void ReturnLettersToUnsorted()
    {
        while (TargetArea.childCount > 0)
        {
            TargetArea.GetChild(0).SetParent(RandomArea);
        }
    }
}