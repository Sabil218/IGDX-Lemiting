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
    [Tooltip("Memutar efek cipratan setiap kali objek masuk ke wajan (Opsional).")]
    [SerializeField] private ParticleSystem splashEffect;
    [Tooltip("SpriteRenderer kuah wajan yang ingin diubah warnanya/gambarnya (Opsional).")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [Tooltip("Gambar pengganti secara bertahap. Indeks 0 saat bahan ke-1 masuk, indeks 1 saat bahan ke-2, dst.")]
    [SerializeField] private Sprite[] progressiveSprites;

    [Header("Events")]
    [Tooltip("Dipanggil saat satu item berhasil di-drop. Menerima item yang di-drop sebagai parameter.")]
    public UnityEvent<WorldObjectDraggable> OnItemDropped;
    public System.Action<WorldObjectDraggable> OnItemDroppedAction;
    public UnityEvent OnAllItemsReceived;

    private int currentItemCount = 0;

    //Reset item count
    public void ResetZone()
    {
        currentItemCount = 0;
    }

    //Process dropped item tanpa consume (untuk skenario wajan -> piring)
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

    //Process valid dropped items
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
