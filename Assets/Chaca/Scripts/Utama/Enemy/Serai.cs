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

        Debug.Log("SERAI MULAI ATTACK");

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

        Debug.Log("SERAI SELESAI ATTACK");
    }

    // =========================================
    // SPAWN PROJECTILE
    // =========================================

    public void SpawnProjectile()
    {
        if (isDead)
            return;

        if (projectilePrefab == null)
        {
            Debug.LogError(
                "Serai: Projectile Prefab belum diisi!"
            );
            return;
        }

        if (projectileSpawnPoint == null)
        {
            Debug.LogError(
                "Serai: Projectile Spawn Point belum diisi!"
            );
            return;
        }

        if (player == null)
        {
            Debug.LogError(
                "Serai: Player tidak ditemukan!"
            );
            return;
        }

        // Projectile menggunakan rotasi bawaan prefab
        GameObject projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            projectilePrefab.transform.rotation
        );

        SeraiProjectile projectileScript =
            projectile.GetComponent<SeraiProjectile>();

        if (projectileScript == null)
        {
            Debug.LogError(
                "SeraiProjectile tidak ditemukan pada prefab!"
            );

            Destroy(projectile);
            return;
        }

        projectileScript.SetTarget(
            player,
            damage
        );

        Debug.Log(
            "SERAI SPAWN PROJECTILE"
        );
    }

    // =========================================
    // ATTACK ANIMATION FINISHED
    // =========================================

    public void AttackAnimationFinished()
    {
        isAttacking = false;

        Debug.Log(
            "SERAI ATTACK ANIMATION FINISHED"
        );
    }
}