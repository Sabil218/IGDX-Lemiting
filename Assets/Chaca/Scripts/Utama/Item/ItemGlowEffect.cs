using UnityEngine;

public class ItemGlowEffect : MonoBehaviour
{
    [Header("Glow Sprites")]
    public SpriteRenderer glow1;
    public SpriteRenderer glow2;

    [Header("Pulse")]
    public float pulseSpeed = 3f;

    [Header("Glow 1")]
    [Range(0f, 1f)]
    public float glow1MinAlpha = 0.03f;

    [Range(0f, 1f)]
    public float glow1MaxAlpha = 0.35f;

    public float glow1MinScale = 0.9f;
    public float glow1MaxScale = 1.3f;

    [Header("Glow 2")]
    [Range(0f, 1f)]
    public float glow2MinAlpha = 0.1f;

    [Range(0f, 1f)]
    public float glow2MaxAlpha = 0.8f;

    public float glow2MinScale = 0.75f;
    public float glow2MaxScale = 1.1f;

    private Vector3 glow1OriginalScale;
    private Vector3 glow2OriginalScale;

    private Color glow1OriginalColor;
    private Color glow2OriginalColor;

    private void Start()
    {
        if (glow1 != null)
        {
            glow1OriginalScale = glow1.transform.localScale;
            glow1OriginalColor = glow1.color;
        }

        if (glow2 != null)
        {
            glow2OriginalScale = glow2.transform.localScale;
            glow2OriginalColor = glow2.color;
        }
    }

    private void Update()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        if (glow1 != null)
        {
            float alpha1 = Mathf.Lerp(
                glow1MinAlpha,
                glow1MaxAlpha,
                pulse
            );

            float scale1 = Mathf.Lerp(
                glow1MinScale,
                glow1MaxScale,
                pulse
            );

            Color color1 = glow1OriginalColor;
            color1.a = alpha1;

            glow1.color = color1;
            glow1.transform.localScale =
                glow1OriginalScale * scale1;
        }

        if (glow2 != null)
        {
            float alpha2 = Mathf.Lerp(
                glow2MinAlpha,
                glow2MaxAlpha,
                pulse
            );

            float scale2 = Mathf.Lerp(
                glow2MinScale,
                glow2MaxScale,
                pulse
            );

            Color color2 = glow2OriginalColor;
            color2.a = alpha2;

            glow2.color = color2;
            glow2.transform.localScale =
                glow2OriginalScale * scale2;
        }
    }
}