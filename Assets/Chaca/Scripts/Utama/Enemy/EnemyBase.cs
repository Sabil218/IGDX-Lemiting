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
    public float dropHeight = 0.1f;
    public float dropSpread = 0.35f;
    public float dropVelocity = 2f;
    public float dropGravity = 3f;

    protected bool isDead;
    protected bool isAttacking;

    public bool IsDead
    {
        get { return isDead; }
    }

    protected virtual void Awake()
    {
        currentHearts = maxHearts;

        isDead = false;
        isAttacking = false;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public abstract void Attack();

    public virtual void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHearts -= amount;

        if (currentHearts <= 0)
        {
            currentHearts = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isAttacking = false;

        if (animator != null)
        {
            animator.enabled = false;
        }

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        SpriteRenderer[] sprites =
            GetComponentsInChildren<SpriteRenderer>();

        Color[] originalColors =
            new Color[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
            {
                originalColors[i] =
                    sprites[i].color;
            }
        }

        DropItems();

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / fadeDuration
                );

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null)
                    continue;

                Color color =
                    originalColors[i];

                color.a =
                    originalColors[i].a * alpha;

                sprites[i].color = color;
            }

            yield return null;
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null)
                continue;

            Color color =
                originalColors[i];

            color.a = 0f;

            sprites[i].color = color;
        }

        if (battleManager != null)
        {
            battleManager.EnemyDefeated();
        }

        Destroy(gameObject);
    }

    protected virtual void DropItems()
    {
        if (dropItemPrefabs == null)
            return;

        if (dropItemPrefabs.Length == 0)
            return;

        SpriteRenderer spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        Vector3 centerPosition =
            transform.position;

        if (spriteRenderer != null)
        {
            centerPosition =
                spriteRenderer.bounds.center;
        }

        centerPosition.y += dropHeight;

        int itemCount =
            dropItemPrefabs.Length;

        for (int i = 0; i < itemCount; i++)
        {
            if (dropItemPrefabs[i] == null)
                continue;

            float horizontalOffset = 0f;

            if (itemCount == 1)
            {
                horizontalOffset = 0f;
            }
            else if (itemCount == 2)
            {
                if (i == 0)
                {
                    horizontalOffset = -dropSpread;
                }
                else
                {
                    horizontalOffset = dropSpread;
                }
            }
            else
            {
                horizontalOffset =
                    Random.Range(
                        -dropSpread,
                        dropSpread
                    );
            }

            SpawnDropItem(
                centerPosition,
                horizontalOffset,
                dropItemPrefabs[i]
            );
        }
    }

    private void SpawnDropItem(
        Vector3 centerPosition,
        float horizontalOffset,
        GameObject itemPrefab
    )
    {
        Vector3 spawnPosition =
            centerPosition +
            new Vector3(
                horizontalOffset,
                0f,
                0f
            );

        GameObject item =
            Instantiate(
                itemPrefab,
                spawnPosition,
                Quaternion.identity
            );

        Rigidbody2D rb =
            item.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale =
                dropGravity;

            float horizontalVelocity =
                horizontalOffset * 2f;

            rb.velocity =
                new Vector2(
                    horizontalVelocity,
                    dropVelocity
                );
        }
    }
}