using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NewIngredientPopup : MonoBehaviour
{
    [Header("UI")]
    public GameObject popupPanel;

    [Header("Baskets")]
    public GameObject basket1;
    public GameObject basket2;
    public GameObject basket3;

    [Header("Ingredient")]
    public Image ingredientImage;
    public TMP_Text ingredientNameText;

    [Header("Popup Animation")]
    public float bounceDuration = 0.45f;
    public float basketDelay = 0.15f;

    [Header("Item Disappear")]
    public float popupDelay = 0.05f;

    private RectTransform basket1Rect;
    private RectTransform basket2Rect;
    private RectTransform basket3Rect;

    private void Start()
    {
        basket1Rect = basket1.GetComponent<RectTransform>();
        basket2Rect = basket2.GetComponent<RectTransform>();
        basket3Rect = basket3.GetComponent<RectTransform>();

        popupPanel.SetActive(false);

        basket1.SetActive(false);
        basket2.SetActive(false);
        basket3.SetActive(false);
    }

    public void ShowAfterItemDisappear(
        GameObject item,
        string ingredientName,
        Sprite ingredientSprite)
    {
        StartCoroutine(
            ItemDisappearThenPopup(
                item,
                ingredientName,
                ingredientSprite
            )
        );
    }

    private IEnumerator ItemDisappearThenPopup(
        GameObject item,
        string ingredientName,
        Sprite ingredientSprite)
    {
        item.SetActive(false);

        yield return new WaitForSecondsRealtime(popupDelay);

        Destroy(item);

        ingredientNameText.text = ingredientName;
        ingredientImage.sprite = ingredientSprite;

        popupPanel.SetActive(true);

        basket1.SetActive(false);
        basket2.SetActive(false);
        basket3.SetActive(false);

        basket1Rect.localScale = Vector3.zero;
        basket2Rect.localScale = Vector3.zero;
        basket3Rect.localScale = Vector3.zero;

        Time.timeScale = 0f;

        StartCoroutine(BasketSequence());
    }

    private IEnumerator BasketSequence()
    {
        yield return StartCoroutine(BounceBasket(basket1, basket1Rect));

        yield return new WaitForSecondsRealtime(basketDelay);

        yield return StartCoroutine(BounceBasket(basket2, basket2Rect));

        yield return new WaitForSecondsRealtime(basketDelay);

        yield return StartCoroutine(BounceBasket(basket3, basket3Rect));
    }

    private IEnumerator BounceBasket(
        GameObject basket,
        RectTransform basketRect)
    {
        basket.SetActive(true);

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

        while (timer < bounceDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = timer / bounceDuration;

            float scale = 1f;

            for (int i = 0; i < points.Length - 1; i++)
            {
                if (progress >= points[i] &&
                    progress <= points[i + 1])
                {
                    float segmentProgress = Mathf.InverseLerp(
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

        basket1.SetActive(false);
        basket2.SetActive(false);
        basket3.SetActive(false);

        Time.timeScale = 1f;
    }
}