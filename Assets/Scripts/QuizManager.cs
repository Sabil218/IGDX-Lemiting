using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("Question")]
    public Image questionImage;
    public Sprite questionSprite;

    [Header("Answer Buttons")]
    public Button[] answerButtons;

    [Header("Answer Images")]
    public Sprite[] answerSprites = new Sprite[4];

    [Header("Correct Answer")]
    [Range(0, 3)]
    public int correctAnswer;

    [Header("Battle Manager")]
    public BattleManager battleManager;

    void Start()
    {
        questionImage.sprite = questionSprite;

        for (int i = 0; i < answerButtons.Length; i++)
        {

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }
    }

    void CheckAnswer(int selectedAnswer)
    {
        foreach (Button btn in answerButtons)
        {
            btn.interactable = false;
        }

        if (selectedAnswer == correctAnswer)
        {
            battleManager.PlayerAttack();
        }
        else
        {
            battleManager.EnemyAttack();
        }

        gameObject.SetActive(false);
    }
}