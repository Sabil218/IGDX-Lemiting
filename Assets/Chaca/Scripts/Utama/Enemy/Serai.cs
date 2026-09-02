using System.Collections;
using UnityEngine;

public class Serai : EnemyBase
{
    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    [Header("Attack Timing")]
    public float attackDuration = 1f;

    public override void Attack()
    {
        if (isDead)
            return;

        if (isAttacking)
            return;

        isAttacking = true;

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        StartCoroutine(FinishAttack());
    }

    private IEnumerator FinishAttack()
    {
        yield return new WaitForSeconds(attackDuration);

        if (isDead)
            yield break;

        isAttacking = false;
    }

    public void SpawnProjectile()
    {
        if (isDead)
            return;

        if (projectilePrefab == null)
            return;

        if (projectileSpawnPoint == null)
            return;

        if (player == null)
            return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            projectilePrefab.transform.rotation
        );

        SeraiProjectile projectileScript =
            projectile.GetComponent<SeraiProjectile>();

        if (projectileScript == null)
        {
            Destroy(projectile);
            return;
        }

        projectileScript.SetTarget(
            player,
            damage
        );
    }

    public void AttackAnimationFinished()
    {
        isAttacking = false;
    }
}