using UnityEngine;
using System.Collections;

public class ResultRewardPanel : MonoBehaviour
{
    [Header("Content")]
    public GameObject content;

    [Header("Win UI")]
    public GameObject rewardText;
    public GameObject rewardContainer;

    [Header("Lose UI")]
    public GameObject encouragementText;

    [Header("Content Animation")]
    public float contentAnimationDuration = 0.5f;

    [Range(0.1f, 1f)]
    public float startScale = 0.6f;

    private Vector3 contentNormalScale;
    private Vector3 rewardTextNormalScale;
    private Vector3 rewardContainerNormalScale;
    private Vector3 encouragementNormalScale;

    private void Awake()
    {
        if (content != null)
            contentNormalScale = content.transform.localScale;

        if (rewardText != null)
            rewardTextNormalScale =
                rewardText.transform.localScale;

        if (rewardContainer != null)
            rewardContainerNormalScale =
                rewardContainer.transform.localScale;

        if (encouragementText != null)
            encouragementNormalScale =
                encouragementText.transform.localScale;

        HideContent();
    }

    public void StartWinSequence()
    {
        StopAllCoroutines();

        if (content != null)
            content.transform.localScale =
                contentNormalScale;

        HideContent();

        StartCoroutine(WinSequence());
    }

    public void StartLoseSequence()
    {
        StopAllCoroutines();

        if (content != null)
            content.transform.localScale =
                contentNormalScale;

        HideContent();

        StartCoroutine(LoseSequence());
    }

    private IEnumerator WinSequence()
    {
        if (content != null)
        {
            content.SetActive(true);

            yield return StartCoroutine(
                PopupAnimation(
                    content.transform,
                    contentNormalScale,
                    contentAnimationDuration
                )
            );
        }

        if (rewardText != null)
        {
            rewardText.SetActive(true);

            yield return StartCoroutine(
                PopupAnimation(
                    rewardText.transform,
                    rewardTextNormalScale,
                    contentAnimationDuration
                )
            );
        }

        if (rewardContainer != null)
        {
            rewardContainer.SetActive(true);

            yield return StartCoroutine(
                PopupAnimation(
                    rewardContainer.transform,
                    rewardContainerNormalScale,
                    contentAnimationDuration
                )
            );
        }
    }

    private IEnumerator LoseSequence()
    {
        if (content != null)
        {
            content.SetActive(true);

            yield return StartCoroutine(
                PopupAnimation(
                    content.transform,
                    contentNormalScale,
                    contentAnimationDuration
                )
            );
        }

        if (encouragementText != null)
        {
            encouragementText.SetActive(true);

            yield return StartCoroutine(
                PopupAnimation(
                    encouragementText.transform,
                    encouragementNormalScale,
                    contentAnimationDuration
                )
            );
        }
    }

    private IEnumerator PopupAnimation(
        Transform target,
        Vector3 normalScale,
        float duration
    )
    {
        Vector3 startScaleVector =
            normalScale * startScale;

        target.localScale = startScaleVector;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / duration
                );

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            target.localScale =
                Vector3.Lerp(
                    startScaleVector,
                    normalScale,
                    smoothProgress
                );

            yield return null;
        }

        target.localScale = normalScale;
    }

    private void HideContent()
    {
        if (content != null)
        {
            content.SetActive(false);

            content.transform.localScale =
                contentNormalScale;
        }

        if (rewardText != null)
        {
            rewardText.SetActive(false);

            rewardText.transform.localScale =
                rewardTextNormalScale;
        }

        if (rewardContainer != null)
        {
            rewardContainer.SetActive(false);

            rewardContainer.transform.localScale =
                rewardContainerNormalScale;
        }

        if (encouragementText != null)
        {
            encouragementText.SetActive(false);

            encouragementText.transform.localScale =
                encouragementNormalScale;
        }
    }

    public void ResetReward()
    {
        StopAllCoroutines();

        if (content != null)
        {
            content.transform.localScale =
                contentNormalScale;
        }

        HideContent();
    }
}