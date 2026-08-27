using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("Answer Buttons")]
    public Button[] answerButtons;

    [Header("Correct Answer")]
    [Range(0, 3)]
    public int correctAnswer;

    private BattleManager battleManager;
    private QuizManager quizManager;

    private bool answered = false;

    private void Start()
    {
        battleManager = FindFirstObjectByType<BattleManager>();
        quizManager = FindFirstObjectByType<QuizManager>();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }
    }

    public void CheckAnswer(int selectedAnswer)
    {
        if (answered)
            return;

        answered = true;

        foreach (Button btn in answerButtons)
        {
            btn.interactable = false;
        }

        if (selectedAnswer == correctAnswer)
        {
            Debug.Log("Jawaban Benar");
            battleManager.PlayerAttack();
        }
        else
        {
            Debug.Log("Jawaban Salah");
            battleManager.EnemyAttack();
        }

        quizManager.RemoveQuiz();
    }
}