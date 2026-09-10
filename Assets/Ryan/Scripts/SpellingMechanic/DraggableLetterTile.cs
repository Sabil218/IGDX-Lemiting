using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Komponen Draggable Letter
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableLetterTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Visual")]
    [SerializeField] private CandyBitmapTextUGUI letterText;

    public char Letter { get; private set; }

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originParent;
    private Vector3 originLocalPosition;
    private int originSiblingIndex;
    private Canvas rootCanvas;
    public bool isConsumed { get; private set; }

    // Initialize component references
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Setup tile letter and visual
    public void Initialize(char letter)
    {
        Letter = char.ToUpper(letter);
        isConsumed = false;

        if (letterText != null)
        {
            letterText.Text = Letter.ToString();
        }

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }
    }

    // Handle start dragging event
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isConsumed) return;

        originParent = transform.parent;
        originLocalPosition = transform.localPosition;
        originSiblingIndex = transform.GetSiblingIndex();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.75f;

        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform);
        }
        transform.SetAsLastSibling();
    }

    // Handle drag movement
    public void OnDrag(PointerEventData eventData)
    {
        if (isConsumed) return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 globalMousePos))
        {
            rectTransform.position = globalMousePos;
        }
    }

    // Handle end dragging event
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isConsumed) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        transform.SetParent(originParent, false);
        transform.SetSiblingIndex(originSiblingIndex);

        if (originParent != null)
        {
            RectTransform rt = originParent.GetComponent<RectTransform>();
            if (rt != null)
            {
                UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(rt);
            }
        }
    }

    // Hide and consume tile when placed correctly
    public void Consume()
    {
        isConsumed = true;

        if (originParent != null)
        {
            transform.SetParent(originParent, false);
            transform.SetSiblingIndex(originSiblingIndex);
        }

        gameObject.SetActive(false);
    }
}