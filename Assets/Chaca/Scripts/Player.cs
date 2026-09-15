using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    public int maxHearts = 3;
    public int currentHearts;
    public int damage = 20;

    [Header("Health UI")]
    public PlayerHealthUI healthUI;

    [Header("Animator")]
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Boomerang")]
    public GameObject boomerangPrefab;
    public Transform boomerangSpawnPoint;

    private Transform currentAttackTarget;

    public bool IsDead { get; private set; }
    public bool IsBubbleTrapped { get; private set; }

    private void Awake()
    {
        currentHearts = maxHearts;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        if (healthUI != null)
        {
            healthUI.UpdateHearts();
        }
    }

    public IEnumerator Attack(Transform enemy)
    {
        if (IsDead || IsBubbleTrapped)
        {
            yield break;
        }

        if (enemy == null)
        {
            yield break;
        }

        currentAttackTarget = enemy;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return null;
    }

    public void ThrowBoomerangEvent()
    {
        if (currentAttackTarget == null)
        {
            return;
        }

        ThrowBoomerang(currentAttackTarget);
    }

    public void ThrowBoomerang(Transform enemy)
    {
        if (boomerangPrefab == null)
        {
            return;
        }

        if (boomerangSpawnPoint == null)
        {
            return;
        }

        if (enemy == null)
        {
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
            Destroy(boomerangObject);
            return;
        }

        boomerang.SetTarget(
            transform,
            enemy,
            damage
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

        if (healthUI != null)
        {
            healthUI.UpdateHearts();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Hurt");
        }

        if (currentHearts <= 0)
        {
            Die();
        }
    }

    public void SetBubbleTrapped(bool trapped)
    {
        if (IsDead)
            return;

        IsBubbleTrapped = trapped;

        if (animator != null)
        {
            animator.SetBool("isBubbleTrapped", trapped);
        }

        if (trapped)
        {
            if (animator != null)
            {
                animator.ResetTrigger("Hurt");
                animator.SetTrigger("Hurt");
            }
        }
    }

    public IEnumerator MoveTo(Transform target)
    {
        if (target == null)
        {
            yield break;
        }

        SetBubbleTrapped(false);

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
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        IsBubbleTrapped = false;

        if (animator != null)
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isBubbleTrapped", false);
        }
    }

    public void ResetHP()
    {
        currentHearts = maxHearts;
        IsDead = false;
        IsBubbleTrapped = false;
        currentAttackTarget = null;

        if (healthUI != null)
        {
            healthUI.UpdateHearts();
        }

        if (animator != null)
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isBubbleTrapped", false);
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Attack");
        }
    }
}