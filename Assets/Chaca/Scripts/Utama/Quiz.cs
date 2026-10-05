using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Quiz : MonoBehaviour
{
    [Header("Answer Buttons")]
    public Button[] answerButtons;

    [Header("Correct Answer")]
    [Range(0, 3)]
    public int correctAnswer;

    [Header("Answer Sound")]
    public AudioClip correctSound;
    public AudioClip wrongSound;

    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("Button Press Effect")]
    [Range(0.7f, 1f)]
    public float pressedScale = 0.9f;

    public float pressDuration = 0.08f;

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

            // Efek tombol ditekan
            AddButtonPressEffect(answerButtons[i]);

            // Check answer
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

            PlaySound(correctSound);

            battleManager.PlayerAttack();
        }
        else
        {
            Debug.Log("Jawaban Salah");

            PlaySound(wrongSound);

            battleManager.EnemyAttack();
        }

        quizManager.RemoveQuiz();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(
            clip,
            Camera.main != null
                ? Camera.main.transform.position
                : Vector3.zero,
            soundVolume
        );
    }

    private void AddButtonPressEffect(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        // Saat tombol ditekan
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;

        pointerDown.callback.AddListener((data) =>
        {
            StartCoroutine(
                ScaleButton(
                    button.transform,
                    pressedScale
                )
            );
        });

        trigger.triggers.Add(pointerDown);

        // Saat tombol dilepas
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;

        pointerUp.callback.AddListener((data) =>
        {
            StartCoroutine(
                ScaleButton(
                    button.transform,
                    1f
                )
            );
        });

        trigger.triggers.Add(pointerUp);
    }

    private IEnumerator ScaleButton(
        Transform buttonTransform,
        float targetScale
    )
    {
        Vector3 startScale = buttonTransform.localScale;
        Vector3 target = Vector3.one * targetScale;

        float timer = 0f;

        while (timer < pressDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(timer / pressDuration);

            buttonTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    target,
                    progress
                );

            yield return null;
        }

        buttonTransform.localScale = target;
    }
}