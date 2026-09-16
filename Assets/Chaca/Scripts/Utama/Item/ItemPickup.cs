using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private bool pickedUp;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp)
            return;

        if (!other.CompareTag("Player"))
            return;

        pickedUp = true;

        Destroy(transform.root.gameObject);
    }
}