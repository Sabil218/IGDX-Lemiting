using UnityEngine;

public class MapProgressBar : MonoBehaviour
{
    [Header("PLAYER")]
    public Transform player;

    [Header("MAP")]
    public Transform startPoint;
    public Transform finishPoint;

    [Header("PROGRESS BAR")]
    public RectTransform background;
    public RectTransform fillMask;
    public RectTransform fill;
    public RectTransform characterIcon;

    [Header("SETTINGS")]
    public bool onlyMoveForward = true;

    private float startX;
    private float finishX;
    private float currentProgress = 0f;
    private float barWidth;

    void Start()
    {
        if (startPoint == null || finishPoint == null || background == null || fillMask == null)
            return;

        startX = startPoint.position.x;
        finishX = finishPoint.position.x;

        barWidth = background.rect.width;

        currentProgress = 0f;

        UpdateVisual();
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
            Vector3 progressEndWorldPosition = fillMask.TransformPoint(
                new Vector3(
                    visibleWidth,
                    fillMask.rect.height,
                    0f
                )
            );

            Vector3 iconLocalPosition =
                background.parent.InverseTransformPoint(progressEndWorldPosition);

            characterIcon.localPosition = iconLocalPosition;
        }
    }
}