using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Player")]
    public Player player;

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Enemy Spawn Points")]
    public Transform[] enemySpawnPoints;

    [Header("Player Stop Points")]
    public Transform[] playerStopPoints;

    [Header("Quiz")]
    public QuizManager quizManager;
    public float quizDelay = 1f;

    [Header("Player Movement")]
    public float moveSpeed = 2f;

    private int currentEnemyIndex;
    private GameObject currentEnemy;
    private EnemyBase currentEnemyBase;

    private bool battleBusy;
    private bool changingEnemy;

    private void Start()
    {
        currentEnemyIndex = 0;

        SpawnCurrentEnemy();

        StartCoroutine(StartFirstQuiz());
    }

    private IEnumerator StartFirstQuiz()
    {
        yield return new WaitForSeconds(quizDelay);

        SpawnQuiz();
    }

    private GameObject SpawnCurrentEnemy()
    {
        if (
            enemyPrefabs == null ||
            enemyPrefabs.Length == 0
        )
        {
            return null;
        }

        if (
            currentEnemyIndex >=
            enemyPrefabs.Length
        )
        {
            return null;
        }

        if (
            enemySpawnPoints == null ||
            currentEnemyIndex >=
            enemySpawnPoints.Length
        )
        {
            return null;
        }

        GameObject enemyPrefab =
            enemyPrefabs[currentEnemyIndex];

        Transform spawnPoint =
            enemySpawnPoints[currentEnemyIndex];

        if (enemyPrefab == null)
        {
            return null;
        }

        if (spawnPoint == null)
        {
            return null;
        }

        currentEnemy =
            Instantiate(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        currentEnemy.name =
            enemyPrefab.name;

        currentEnemyBase =
            currentEnemy.GetComponent<EnemyBase>();

        if (currentEnemyBase == null)
        {
            Destroy(currentEnemy);

            currentEnemy = null;

            return null;
        }

        currentEnemyBase.player =
            player.transform;

        currentEnemyBase.battleManager =
            this;

        return currentEnemy;
    }

    public void PlayerAttack()
    {
        if (battleBusy)
            return;

        if (changingEnemy)
            return;

        if (currentEnemyBase == null)
            return;

        if (currentEnemyBase.IsDead)
            return;

        battleBusy = true;

        StartCoroutine(
            PlayerAttackCoroutine()
        );
    }

    private IEnumerator PlayerAttackCoroutine()
    {
        if (currentEnemyBase == null)
        {
            battleBusy = false;
            yield break;
        }

        yield return StartCoroutine(
            player.Attack(
                currentEnemy.transform
            )
        );

        if (currentEnemyBase == null)
        {
            battleBusy = false;
            yield break;
        }

        if (currentEnemyBase.IsDead)
        {
            battleBusy = false;
            yield break;
        }

        yield return new WaitForSeconds(
            quizDelay
        );

        if (
            !changingEnemy &&
            currentEnemyBase != null &&
            !currentEnemyBase.IsDead
        )
        {
            SpawnQuiz();
        }

        battleBusy = false;
    }

    public void EnemyDefeated()
    {
        if (changingEnemy)
            return;

        StartCoroutine(
            EnemyDefeatedCoroutine()
        );
    }

    private IEnumerator EnemyDefeatedCoroutine()
    {
        changingEnemy = true;
        battleBusy = true;

        RemoveQuiz();

        currentEnemy = null;
        currentEnemyBase = null;

        currentEnemyIndex++;

        if (
            currentEnemyIndex >=
            enemyPrefabs.Length
        )
        {
            battleBusy = false;
            yield break;
        }

        GameObject nextEnemy =
            SpawnCurrentEnemy();

        if (nextEnemy == null)
        {
            battleBusy = false;
            yield break;
        }

        if (
            playerStopPoints == null ||
            currentEnemyIndex >=
            playerStopPoints.Length
        )
        {
            battleBusy = false;
            yield break;
        }

        Transform playerStopPoint =
            playerStopPoints[currentEnemyIndex];

        if (playerStopPoint == null)
        {
            battleBusy = false;
            yield break;
        }

        yield return StartCoroutine(
            MovePlayerTo(
                playerStopPoint
            )
        );

        yield return new WaitForSeconds(
            quizDelay
        );

        changingEnemy = false;

        SpawnQuiz();

        battleBusy = false;
    }

    private IEnumerator MovePlayerTo(
        Transform target
    )
    {
        if (target == null)
            yield break;

        float playerY =
            player.transform.position.y;

        SetRunAnimation(true);

        while (
            Mathf.Abs(
                player.transform.position.x -
                target.position.x
            ) > 0.05f
        )
        {
            Vector3 currentPosition =
                player.transform.position;

            float newX =
                Mathf.MoveTowards(
                    currentPosition.x,
                    target.position.x,
                    moveSpeed *
                    Time.deltaTime
                );

            player.transform.position =
                new Vector3(
                    newX,
                    playerY,
                    currentPosition.z
                );

            yield return null;
        }

        player.transform.position =
            new Vector3(
                target.position.x,
                playerY,
                player.transform.position.z
            );

        SetRunAnimation(false);
    }

    private void SetRunAnimation(
        bool running
    )
    {
        if (player == null)
            return;

        if (player.animator == null)
            return;

        player.animator.SetBool(
            "isRun",
            running
        );
    }

    public void EnemyAttack()
    {
        if (battleBusy)
            return;

        if (changingEnemy)
            return;

        if (currentEnemyBase == null)
            return;

        if (currentEnemyBase.IsDead)
            return;

        battleBusy = true;

        StartCoroutine(
            EnemyAttackCoroutine()
        );
    }

    private IEnumerator EnemyAttackCoroutine()
    {
        if (currentEnemyBase == null)
        {
            battleBusy = false;
            yield break;
        }

        currentEnemyBase.Attack();

        while (
            currentEnemyBase != null &&
            currentEnemyBase.IsAttacking
        )
        {
            yield return null;
        }

        RemoveQuiz();

        yield return new WaitForSeconds(
            quizDelay
        );

        if (
            !changingEnemy &&
            currentEnemyBase != null &&
            !currentEnemyBase.IsDead
        )
        {
            SpawnQuiz();
        }

        battleBusy = false;
    }

    private void SpawnQuiz()
    {
        if (quizManager == null)
            return;

        if (changingEnemy)
            return;

        if (currentEnemyBase == null)
            return;

        if (currentEnemyBase.IsDead)
            return;

        quizManager.SpawnRandomQuiz();
    }

    private void RemoveQuiz()
    {
        if (quizManager == null)
            return;

        quizManager.RemoveQuiz();
    }
}