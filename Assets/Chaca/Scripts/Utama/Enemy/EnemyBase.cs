using System.Collections;
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
            col.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (battleManager != null)
        {
            battleManager.EnemyDefeated();
        }

        StartCoroutine(FadeOut());
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

    private IEnumerator FadeOut()
    {
        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>();

        if (
            renderers == null ||
            renderers.Length == 0
        )
        {
            Destroy(gameObject);
            yield break;
        }

        Color[] originalColors =
            new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalColors[i] =
                    renderers[i].color;
            }
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                Color color =
                    originalColors[i];

                color.a =
                    originalColors[i].a *
                    alpha;

                renderers[i].color =
                    color;
            }

            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Color color =
                originalColors[i];

            color.a = 0f;

            renderers[i].color =
                color;
        }

        Destroy(gameObject);
    }

    protected bool HasParameter(
        string parameterName
    )
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