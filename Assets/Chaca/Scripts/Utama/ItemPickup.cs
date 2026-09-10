using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    public string itemName = "Item";

    [Header("Pickup")]
    public bool destroyOnPickup = true;

    private bool pickedUp;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp)
            return;

        if (!other.CompareTag("Player"))
            return;

        pickedUp = true;

        PickupItem(other.gameObject);

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }

    private void PickupItem(GameObject player)
    {
    }
}