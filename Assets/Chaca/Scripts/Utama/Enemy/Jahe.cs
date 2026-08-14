using UnityEngine;

public class Jahe : EnemyBase
{
    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    public override void Attack()
    {
        if (isDead)
            return;

        if (isAttacking)
            return;

        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void SpawnProjectile()
    {
        if (isDead)
            return;

        if (projectilePrefab == null)
        {
            return;
        }

        if (projectileSpawnPoint == null)
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                Quaternion.identity
            );

        JaheProjectile projectileScript =
            projectile.GetComponent<JaheProjectile>();

        if (projectileScript == null)
        {
            return;
        }

        projectileScript.SetTarget(
            player,
            damage
        );
    }

    public void AttackAnimationFinished()
    {
        if (isDead)
            return;

        isAttacking = false;
    }
}