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

    // =========================================
    // TAKE DAMAGE
    // =========================================

    public virtual void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHearts -= amount;

        if (currentHearts < 0)
        {
            currentHearts = 0;
        }

        Debug.Log(
            gameObject.name +
            " Heart: " +
            currentHearts +
            "/" +
            maxHearts
        );

        if (currentHearts <= 0)
        {
            Die();
            return;
        }

        TriggerHurt();
    }

    // =========================================
    // HURT
    // =========================================

    protected virtual void TriggerHurt()
    {
        if (animator == null)
            return;

        if (!HasParameter("Hurt"))
            return;

        animator.SetTrigger("Hurt");
    }

    // =========================================
    // DIE
    // =========================================

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isAttacking = false;

        Debug.Log(
            gameObject.name +
            " MATI"
        );

        // =========================================
        // MATIKAN COLLIDER
        // =========================================

        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // =========================================
        // MATIKAN ATTACK
        // =========================================

        if (animator != null)
        {
            animator.enabled = false;
        }

        // =========================================
        // BERI TAHU BATTLE MANAGER
        // =========================================

        if (battleManager != null)
        {
            battleManager.EnemyDefeated();
        }

        // =========================================
        // FADE
        // =========================================

        StartCoroutine(
            FadeOut()
        );
    }

    // =========================================
    // FADE OUT
    // =========================================

    private IEnumerator FadeOut()
    {
        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>();

        // Kalau tidak ada SpriteRenderer,
        // langsung hancurkan
        if (
            renderers == null ||
            renderers.Length == 0
        )
        {
            Destroy(gameObject);
            yield break;
        }

        // Simpan warna awal
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

        // =========================================
        // PASTIKAN TRANSPARAN
        // =========================================

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

        Debug.Log(
            gameObject.name +
            " FADE SELESAI"
        );

        // =========================================
        // HANCURKAN ENEMY
        // =========================================

        Destroy(gameObject);
    }

    // =========================================
    // CHECK ANIMATOR PARAMETER
    // =========================================

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

    // =========================================
    // ATTACK
    // =========================================

    public abstract void Attack();
}