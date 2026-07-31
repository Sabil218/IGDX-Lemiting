using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static DragHandler draggedItem;
    [SerializeField] TMPro.TextMeshProUGUI letterText;

    private Vector3 originalPos;
    private Transform originParent;
    private int originalIndex;

    public string Letter { get; private set; }

    public void LetterInit(Transform parent, string letter)
    {
        Letter = letter;
        transform.SetParent(parent);
        letterText.SetText(letter);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = transform.position;
        originParent = transform.parent;

        //Make sure not reshuffle
        originalIndex = transform.GetSiblingIndex();

        draggedItem = this;
        GetComponent<CanvasGroup>().blocksRaycasts = false;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draggedItem = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (transform.parent == transform.root)
        {
            transform.SetParent(originParent);

            transform.SetSiblingIndex(originalIndex);
        }
    }
}