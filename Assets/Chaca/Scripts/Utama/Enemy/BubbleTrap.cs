using UnityEngine;

public class BubbleTrap : MonoBehaviour
{
    [Header("Bubble Settings")]
    public int damage = 1;
    public float duration = 2f;

    private Player trappedPlayer;

    public void SetPlayer(Player player)
    {
        trappedPlayer = player;

        if (trappedPlayer != null)
        {
            trappedPlayer.TakeDamage(damage);
            trappedPlayer.SetBubbleTrapped(true);
        }
    }

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnDestroy()
    {
        if (trappedPlayer != null)
        {
            trappedPlayer.SetBubbleTrapped(false);
        }
    }
}