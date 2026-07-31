using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //Detect Dragged Item and Make a DropZone
        if (DragHandler.draggedItem != null)
        {
            DragHandler.draggedItem.transform.SetParent(this.transform);
            WordManager.instance.EvaluateBoard();
        }
    }
}