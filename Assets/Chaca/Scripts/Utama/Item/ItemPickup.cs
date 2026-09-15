using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    public string itemName = "Item";
    public Sprite itemIcon;

    private bool pickedUp;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (BattleItemCollector.Instance == null)
            return;

        if (BattleItemCollector.Instance.HasItem(itemName))
        {
            pickedUp = true;
            Destroy(transform.root.gameObject);
            return;
        }

        BattleItemCollector.Instance.CollectItem(
            itemName,
            itemIcon
        );

        pickedUp = true;

        Destroy(transform.root.gameObject);
    }
}