using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 100;
    public int currentHP;
    public int damage = 15;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        ResetHP();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead)
            return;

        currentHP -= amount;

        if (currentHP < 0)
            currentHP = 0;

        Debug.Log(gameObject.name + " HP : " + currentHP);

        // animator.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        IsDead = true;

        Debug.Log(gameObject.name + " Mati");

        // animator.SetTrigger("Die");
    }

    public void ResetHP()
    {
        currentHP = maxHP;
        IsDead = false;
    }
}