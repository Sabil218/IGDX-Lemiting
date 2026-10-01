using UnityEngine;
using UnityEngine.EventSystems;

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

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Sets the letter text and prepares the tile to be dragged.
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

    // Handles picking up the tile and detaching it from the layout grid.
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

    // Hides and disables the tile after it has been successfully matched on the board.
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