using UnityEngine;

public class Ikan : EnemyBase
{
    [Header("Bubble Attack")]
    public GameObject bubblePrefab;

    private GameObject currentBubble;

    public override void Attack()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    // Animation Event
    public void SpawnBubbleEvent()
    {
        if (isDead)
            return;

        if (bubblePrefab == null)
            return;

        if (player == null)
            return;

        // Kalau masih ada bubble lama, jangan buat baru
        if (currentBubble != null)
            return;

        GameObject bubble = Instantiate(
            bubblePrefab,
            player.position,
            Quaternion.identity
        );

        currentBubble = bubble;

        BubbleTrap bubbleTrap =
            bubble.GetComponent<BubbleTrap>();

        Player playerScript =
            player.GetComponent<Player>();

        if (bubbleTrap != null && playerScript != null)
        {
            bubbleTrap.SetPlayer(playerScript);
        }
    }

    // Animation Event
    public void ReleaseBubbleEvent()
    {
        if (currentBubble != null)
        {
            Destroy(currentBubble);
            currentBubble = null;
        }

        isAttacking = false;
    }
}