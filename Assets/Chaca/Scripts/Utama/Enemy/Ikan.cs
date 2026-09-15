using System.Collections;
using UnityEngine;

public class Ikan : EnemyBase
{
    [Header("Bubble Attack")]
    public GameObject bubblePrefab;

    public float attackDelay = 0.5f;

    public override void Attack()
    {
        if (isDead || isAttacking)
            return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(attackDelay);

        SpawnBubbleOnPlayer();

        isAttacking = false;
    }

    private void SpawnBubbleOnPlayer()
    {
        if (bubblePrefab == null || player == null)
            return;

        GameObject bubble = Instantiate(
            bubblePrefab,
            player.position,
            Quaternion.identity
        );

        BubbleTrap bubbleTrap = bubble.GetComponent<BubbleTrap>();
        Player playerScript = player.GetComponent<Player>();

        if (bubbleTrap != null && playerScript != null)
        {
            bubbleTrap.SetPlayer(playerScript);
        }
    }
}