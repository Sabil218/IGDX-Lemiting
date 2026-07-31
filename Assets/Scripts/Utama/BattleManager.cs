using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public int playerHP = 100;
    public int playerDamage = 20;

    [Header("Enemy")]
    public Transform enemy;
    public int enemyHP = 100;
    public int enemyDamage = 15;

    [Header("Attack Settings")]
    public float attackDelay = 0.5f;

    [Header("Quiz")]
    public QuizManager quizManager;

    [Header("Player Movement")]
    public float moveSpeed = 3f;
    public Transform nextPoint;

    public void PlayerAttack()
    {
        StartCoroutine(PlayerAttackCoroutine());
    }

    IEnumerator PlayerAttackCoroutine()
    {
        Debug.Log("Player Attack");

        // playerAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        enemyHP -= playerDamage;

        if (enemyHP < 0)
            enemyHP = 0;

        Debug.Log("Enemy HP : " + enemyHP);

        // enemyAnimator.SetTrigger("Hit");

        if (enemyHP <= 0)
        {
            Debug.Log("Enemy Mati");

            // enemyAnimator.SetTrigger("Die");

            quizManager.RemoveQuiz();

            yield return new WaitForSeconds(1f);

            enemy.gameObject.SetActive(false);

            StartCoroutine(PlayerMoveToNextPoint());
        }
        else
        {
            quizManager.RemoveQuiz();

            yield return new WaitForSeconds(0.5f);

            quizManager.SpawnRandomQuiz();
        }
    }

    public void EnemyAttack()
    {
        StartCoroutine(EnemyAttackCoroutine());
    }

    IEnumerator EnemyAttackCoroutine()
    {
        Debug.Log("Enemy Attack");

        // enemyAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        playerHP -= enemyDamage;

        if (playerHP < 0)
            playerHP = 0;

        Debug.Log("Player HP : " + playerHP);

        // playerAnimator.SetTrigger("Hit");

        if (playerHP <= 0)
        {
            Debug.Log("Player Mati");

            // playerAnimator.SetTrigger("Die");
        }
        else
        {
            quizManager.RemoveQuiz();

            yield return new WaitForSeconds(0.5f);

            quizManager.SpawnRandomQuiz();
        }
    }

    IEnumerator PlayerMoveToNextPoint()
    {
        Debug.Log("Player Jalan");

        // playerAnimator.SetBool("Walk", true);

        while (Vector2.Distance(player.position, nextPoint.position) > 0.05f)
        {
            player.position = Vector2.MoveTowards(
                player.position,
                nextPoint.position,
                moveSpeed * Time.deltaTime);

            yield return null;
        }

        // playerAnimator.SetBool("Walk", false);

        Debug.Log("Player Sampai");
    }
}