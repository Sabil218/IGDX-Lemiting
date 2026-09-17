using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 20f;

    [Header("Scale")]
    public float minScale = 0.9f;
    public float maxScale = 1.1f;
    public float scaleSpeed = 2f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Berputar
        rectTransform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.unscaledDeltaTime
        );

        // Membesar dan mengecil
        float scale = Mathf.Lerp(
            minScale,
            maxScale,
            (Mathf.Sin(Time.unscaledTime * scaleSpeed) + 1f) / 2f
        );

        rectTransform.localScale = new Vector3(
            scale,
            scale,
            1f
        );
    }
}