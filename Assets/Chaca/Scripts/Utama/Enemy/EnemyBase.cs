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
    public GameObject[] dropItemPrefabs;
    public float dropHeight = 0.5f;
    public float dropHorizontalSpacing = 0.7f;
    public float dropRandomHorizontal = 0.3f;
    public float dropRandomVertical = 0.2f;
    public float dropForceHorizontal = 1.5f;
    public float dropForceVertical = 2f;

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

        DropItems();

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

    protected virtual void DropItems()
    {
        if (dropItemPrefabs == null ||
            dropItemPrefabs.Length == 0)
        {
            return;
        }

        SpriteRenderer spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        Vector3 basePosition = transform.position;

        if (spriteRenderer != null)
        {
            basePosition = spriteRenderer.bounds.center;
        }

        for (int i = 0; i < dropItemPrefabs.Length; i++)
        {
            if (dropItemPrefabs[i] == null)
                continue;

            float direction;

            if (dropItemPrefabs.Length == 1)
            {
                direction = Random.value < 0.5f ? -1f : 1f;
            }
            else
            {
                direction = i == 0 ? -1f : 1f;
            }

            float horizontalOffset =
                direction * dropHorizontalSpacing;

            horizontalOffset +=
                Random.Range(
                    -dropRandomHorizontal,
                    dropRandomHorizontal
                );

            float verticalOffset =
                Random.Range(
                    -dropRandomVertical,
                    dropRandomVertical
                );

            Vector3 spawnPosition =
                basePosition +
                new Vector3(
                    horizontalOffset,
                    dropHeight + verticalOffset,
                    0f
                );

            GameObject item =
                Instantiate(
                    dropItemPrefabs[i],
                    spawnPosition,
                    Quaternion.identity
                );

            Rigidbody2D rb =
                item.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                float randomHorizontalForce =
                    Random.Range(
                        dropForceHorizontal * 0.7f,
                        dropForceHorizontal * 1.3f
                    );

                float randomVerticalForce =
                    Random.Range(
                        dropForceVertical * 0.8f,
                        dropForceVertical * 1.2f
                    );

                rb.AddForce(
                    new Vector2(
                        direction * randomHorizontalForce,
                        randomVerticalForce
                    ),
                    ForceMode2D.Impulse
                );
            }
        }
    }

    private IEnumerator FadeOut()
    {
        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>();

        if (renderers == null ||
            renderers.Length == 0)
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