using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class CookingDropZone : MonoBehaviour
{
    [Header("Validation")]
    [SerializeField] private string acceptedTag = "BahanMentah";

    [Header("Capacity")]
    [SerializeField] private int requiredItemCount = 1;

    [Header("Visual Feedback (Tercampur)")]
    [SerializeField] private ParticleSystem splashEffect;
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private Sprite[] progressiveSprites;

    [Header("Events")]
    public UnityEvent<WorldObjectDraggable> OnItemDropped;
    public System.Action<WorldObjectDraggable> OnItemDroppedAction;
    public UnityEvent OnAllItemsReceived;

    private int currentItemCount = 0;

    public void ResetZone()
    {
        currentItemCount = 0;
    }

    // Accepts an item into the zone but does not destroy or hide it (useful for tools like spatulas).
    public void HandleDropNoConsume(WorldObjectDraggable draggableItem)
    {
        if (draggableItem == null) return;

        if (draggableItem.gameObject.CompareTag(acceptedTag))
        {
            currentItemCount++;

            if (splashEffect != null)
            {
                splashEffect.Play();
            }

            if (targetSpriteRenderer != null && progressiveSprites != null && progressiveSprites.Length > 0)
            {
                int index = Mathf.Clamp(currentItemCount - 1, 0, progressiveSprites.Length - 1);
                targetSpriteRenderer.sprite = progressiveSprites[index];
            }

            OnItemDropped?.Invoke(draggableItem);
            OnItemDroppedAction?.Invoke(draggableItem);

            if (currentItemCount >= requiredItemCount)
            {
                OnAllItemsReceived?.Invoke();
            }
        }
    }

    // Accepts an item and consumes/hides it (useful for ingredients going into a pan).
    public void HandleDrop(WorldObjectDraggable draggableItem)
    {
        if (draggableItem == null || draggableItem.isConsumed) return;

        if (draggableItem.gameObject.CompareTag(acceptedTag))
        {
            draggableItem.Consume(this.transform);
            currentItemCount++;

            if (splashEffect != null)
            {
                splashEffect.Play();
            }

            if (targetSpriteRenderer != null && progressiveSprites != null && progressiveSprites.Length > 0)
            {
                int index = Mathf.Clamp(currentItemCount - 1, 0, progressiveSprites.Length - 1);
                targetSpriteRenderer.sprite = progressiveSprites[index];
            }

            OnItemDropped?.Invoke(draggableItem);
            OnItemDroppedAction?.Invoke(draggableItem);

            if (currentItemCount >= requiredItemCount)
            {
                OnAllItemsReceived?.Invoke();
            }
        }
    }
}
