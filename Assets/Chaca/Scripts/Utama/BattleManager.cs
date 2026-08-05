using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public int playerHP = 100;
    public int playerDamage = 20;

    [Header("Enemies")]
    public Enemy[] enemies;
    private Enemy currentEnemy;
    private int currentEnemyIndex = 0;

    [Header("Attack Settings")]
    public float attackDelay = 0.5f;

    [Header("Quiz")]
    public QuizManager quizManager;

    [Header("Player Movement")]
    public float moveSpeed = 3f;
    public Transform[] battlePoints;

    private void Start()
    {
        currentEnemy = enemies[currentEnemyIndex];

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].gameObject.SetActive(i == 0);
        }
    }

    public void PlayerAttack()
    {
        StartCoroutine(PlayerAttackCoroutine());
    }

    IEnumerator PlayerAttackCoroutine()
    {
        Debug.Log("Player Attack");

        // playerAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        currentEnemy.TakeDamage(playerDamage);

        if (currentEnemy.currentHP <= 0)
        {
            quizManager.RemoveQuiz();

            yield return new WaitForSeconds(1f);

            currentEnemyIndex++;

            if (currentEnemyIndex >= enemies.Length)
            {
                Debug.Log("Semua Enemy Sudah Mati");
                yield break;
            }

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

        playerHP -= currentEnemy.damage;

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

        while (Vector2.Distance(player.position, battlePoints[currentEnemyIndex].position) > 0.05f)
        {
            player.position = Vector2.MoveTowards(
                player.position,
                battlePoints[currentEnemyIndex].position,
                moveSpeed * Time.deltaTime);

            yield return null;
        }

        // playerAnimator.SetBool("Walk", false);

        currentEnemy.gameObject.SetActive(false);

        currentEnemy = enemies[currentEnemyIndex];

        currentEnemy.gameObject.SetActive(true);

        currentEnemy.ResetHP();

        quizManager.SpawnRandomQuiz();

        Debug.Log("Battle Selanjutnya");
    }
}