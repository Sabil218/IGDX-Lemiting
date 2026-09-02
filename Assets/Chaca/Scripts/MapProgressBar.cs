using UnityEngine;
using UnityEngine.UI;

public class MapProgressBar : MonoBehaviour
{
    [Header("PLAYER")]
    public Transform player;

    [Header("MAP")]
    public Transform startPoint;
    public Transform finishPoint;

    [Header("PROGRESS BAR")]
    public RectTransform progressBar;
    public RectTransform fillMask;
    public RectTransform fill;
    public RectTransform characterIcon;

    [Header("TOGGLE")]
    public Toggle progressToggle;

    [Header("SETTINGS")]
    public bool onlyMoveForward = true;

    private float startX;
    private float finishX;
    private float currentProgress = 0f;
    private float barWidth;

    void Start()
    {
        startX = startPoint.position.x;
        finishX = finishPoint.position.x;

        barWidth = progressBar.rect.width;

        currentProgress = 0f;

        UpdateVisual();

        if (progressToggle != null)
        {
            progressToggle.isOn = true;
            progressToggle.onValueChanged.AddListener(SetProgressBarVisible);
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float progress = Mathf.InverseLerp(
            startX,
            finishX,
            player.position.x
        );

        progress = Mathf.Clamp01(progress);

        if (onlyMoveForward)
        {
            if (progress > currentProgress)
            {
                currentProgress = progress;
            }
        }
        else
        {
            currentProgress = progress;
        }

        UpdateVisual();
    }

    void UpdateVisual()
    {
        float visibleWidth = barWidth * currentProgress;

        Vector2 maskSize = fillMask.sizeDelta;
        maskSize.x = visibleWidth;
        fillMask.sizeDelta = maskSize;

        if (characterIcon != null)
        {
            Vector2 iconPosition = characterIcon.anchoredPosition;
            iconPosition.x = barWidth * currentProgress;
            characterIcon.anchoredPosition = iconPosition;
        }
    }

    void SetProgressBarVisible(bool isVisible)
    {
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(isVisible);
        }
    }
}