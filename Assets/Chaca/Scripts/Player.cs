using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    public int maxHearts = 3;
    public int currentHearts;
    public int damage = 20;

    [Header("Animator")]
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Boomerang")]
    public GameObject boomerangPrefab;
    public Transform boomerangSpawnPoint;

    private Transform currentAttackTarget;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        currentHearts = maxHearts;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public IEnumerator Attack(Transform enemy)
    {
        if (IsDead)
        {
            Debug.Log("Player sudah mati.");
            yield break;
        }

        if (enemy == null)
        {
            Debug.LogError("Target Enemy kosong saat Player Attack!");
            yield break;
        }

        currentAttackTarget = enemy;

        Debug.Log("Target Attack Player : " + currentAttackTarget.name);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return null;
    }

    public void ThrowBoomerangEvent()
    {
        Debug.Log("Animation Event: Throw Boomerang");

        if (currentAttackTarget == null)
        {
            Debug.LogError("Target Boomerang kosong!");
            return;
        }

        Debug.Log(
            "Boomerang menargetkan : " +
            currentAttackTarget.name
        );

        ThrowBoomerang(currentAttackTarget);
    }

    public void ThrowBoomerang(Transform enemy)
    {
        if (boomerangPrefab == null)
        {
            Debug.LogError("Boomerang Prefab belum diisi!");
            return;
        }

        if (boomerangSpawnPoint == null)
        {
            Debug.LogError("Boomerang Spawn Point belum diisi!");
            return;
        }

        if (enemy == null)
        {
            Debug.LogError("Target Enemy kosong!");
            return;
        }

        GameObject boomerangObject = Instantiate(
            boomerangPrefab,
            boomerangSpawnPoint.position,
            Quaternion.identity
        );

        PlayerBoomerang boomerang =
            boomerangObject.GetComponent<PlayerBoomerang>();

        if (boomerang == null)
        {
            Debug.LogError(
                "PlayerBoomerang tidak ditemukan di prefab Boomerang!"
            );

            Destroy(boomerangObject);
            return;
        }

        boomerang.SetTarget(
            transform,
            enemy,
            damage
        );

        Debug.Log(
            "Boomerang berhasil dilempar ke " +
            enemy.name
        );
    }

    public void TakeDamage(int amount)
    {
        if (IsDead)
            return;

        currentHearts -= amount;

        if (currentHearts < 0)
        {
            currentHearts = 0;
        }

        Debug.Log(
            "Player Heart : " +
            currentHearts +
            "/" +
            maxHearts
        );

        if (animator == null)
        {
            Debug.LogError("Animator Player belum diisi!");
        }
        else
        {
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Hurt");

            Debug.Log("Hurt Trigger dipanggil");
        }

        if (currentHearts <= 0)
        {
            Die();
        }
    }

    public IEnumerator MoveTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogError("Target MoveTo kosong!");
            yield break;
        }

        Debug.Log("Player Mulai Jalan");

        float fixedY = transform.position.y;

        if (animator != null)
        {
            animator.SetBool("isRun", true);
        }

        while (Mathf.Abs(
            transform.position.x - target.position.x
        ) > 0.05f)
        {
            Vector3 targetPosition = new Vector3(
                target.position.x,
                fixedY,
                transform.position.z
            );

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = new Vector3(
            target.position.x,
            fixedY,
            transform.position.z
        );

        if (animator != null)
        {
            animator.SetBool("isRun", false);
        }

        Debug.Log("Player Sampai");
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        if (animator != null)
        {
            animator.SetBool("isRun", false);
        }

        Debug.Log("Player Mati");
    }

    public void ResetHP()
    {
        currentHearts = maxHearts;
        IsDead = false;
        currentAttackTarget = null;

        if (animator != null)
        {
            animator.SetBool("isRun", false);
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Attack");
        }

        Debug.Log(
            "Player Heart Reset : " +
            currentHearts +
            "/" +
            maxHearts
        );
    }
}