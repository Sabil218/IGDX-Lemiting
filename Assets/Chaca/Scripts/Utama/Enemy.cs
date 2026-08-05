using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 100;
    public int currentHP;
    public int damage = 15;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
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

    public void Die()
    {
        Debug.Log(gameObject.name + " Mati");

        // animator.SetTrigger("Die");

        gameObject.SetActive(false);
    }

    public void ResetHP()
    {
        currentHP = maxHP;
    }
}
