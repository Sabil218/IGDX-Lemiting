using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHearts = 3;
    public int currentHearts = 3;
    public int damage = 1;

    [Header("Animator")]
    public Animator animator;

    [Header("Battle")]
    public BattleManager battleManager;
    public Transform player;

    [Header("Death Fade")]
    public float fadeDuration = 1f;

    [Header("Item Drop")]
    public GameObject dropItemPrefab;
    public float dropHeight = 0.5f;

    protected bool isDead;
    protected bool isAttacking;

    public bool IsDead => isDead;
    public bool IsAttacking => isAttacking;

    protected virtual void Awake()
    {
        currentHearts = maxHearts;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHearts -= amount;

        if (currentHearts < 0)
        {
            currentHearts = 0;
        }

        if (currentHearts <= 0)
        {
            Die();
            return;
        }

        TriggerHurt();
    }

    protected virtual void TriggerHurt()
    {
        if (animator == null)
            return;

        if (!HasParameter("Hurt"))
            return;

        animator.SetTrigger("Hurt");
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isAttacking = false;

        DropItem();

        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (battleManager != null)
        {
            battleManager.EnemyDefeated();
        }
    }

    protected virtual void DropItem()
    {
        if (dropItemPrefab == null)
            return;

        Vector3 spawnPosition =
            transform.position +
            Vector3.up * dropHeight;

        Instantiate(
            dropItemPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }

    protected bool HasParameter(string parameterName)
    {
        if (animator == null)
            return false;

        foreach (
            AnimatorControllerParameter parameter
            in animator.parameters
        )
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }

    public abstract void Attack();
}