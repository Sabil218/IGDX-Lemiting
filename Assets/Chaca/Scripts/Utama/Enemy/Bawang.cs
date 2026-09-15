using System.Collections;
using UnityEngine;

public class Bawang : EnemyBase
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    [Header("Attack Timing")]
    public float projectileDelay = 0.4f;
    public float attackDuration = 1.2f;

    public override void Attack()
    {
        if (isDead)
            return;

        if (isAttacking)
            return;

        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(projectileDelay);

        ThrowProjectile();

        float remainingTime =
            attackDuration - projectileDelay;

        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(
                remainingTime
            );
        }

        isAttacking = false;
    }

    private void ThrowProjectile()
    {
        if (projectilePrefab == null)
            return;

        if (projectileSpawnPoint == null)
            return;

        if (player == null)
            return;

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        BawangProjectile projectile =
            projectileObject.GetComponent<BawangProjectile>();

        if (projectile == null)
        {
            Destroy(projectileObject);
            return;
        }

        projectile.SetTarget(
            player,
            damage
        );
    }
}