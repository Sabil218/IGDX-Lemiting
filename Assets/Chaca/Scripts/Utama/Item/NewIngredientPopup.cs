using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NewIngredientPopup : MonoBehaviour
{
    [Header("UI")]
    public GameObject popupPanel;
    public GameObject basket;
    public Image ingredientImage;
    public TMP_Text ingredientNameText;

    [Header("Popup Animation")]
    public float popupDuration = 0.45f;

    private RectTransform basketRect;

    private void Start()
    {
        basketRect = basket.GetComponent<RectTransform>();

        popupPanel.SetActive(false);
    }

    public void ShowIngredient(string ingredientName, Sprite ingredientSprite)
    {
        ingredientNameText.text = ingredientName;
        ingredientImage.sprite = ingredientSprite;

        popupPanel.SetActive(true);

        basketRect.localScale = Vector3.zero;

        StopAllCoroutines();
        StartCoroutine(BasketPopupAnimation());

        Time.timeScale = 0f;
    }

    private IEnumerator BasketPopupAnimation()
    {
        float timer = 0f;

        float[] scales =
        {
            0f,
            1.2f,
            0.85f,
            1.08f,
            0.96f,
            1f
        };

        float[] points =
        {
            0f,
            0.35f,
            0.55f,
            0.72f,
            0.86f,
            1f
        };

        while (timer < popupDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = timer / popupDuration;

            float scale = 1f;

            for (int i = 0; i < points.Length - 1; i++)
            {
                if (progress >= points[i] && progress <= points[i + 1])
                {
                    float segmentProgress =
                        Mathf.InverseLerp(
                            points[i],
                            points[i + 1],
                            progress
                        );

                    segmentProgress = Mathf.SmoothStep(
                        0f,
                        1f,
                        segmentProgress
                    );

                    scale = Mathf.Lerp(
                        scales[i],
                        scales[i + 1],
                        segmentProgress
                    );

                    break;
                }
            }

            basketRect.localScale = Vector3.one * scale;

            yield return null;
        }

        basketRect.localScale = Vector3.one;
    }

    private void Update()
    {
        if (!popupPanel.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            ClosePopup();
        }
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}