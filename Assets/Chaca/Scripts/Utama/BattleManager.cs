using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Enemy Fade")]
    public float enemyFadeDuration = 1f;

    [Header("Camera")]
    public CameraFollow cameraFollow;

    [Header("Win / Lose Panel")]
    public GameObject winningPanel;
    public GameObject loosePanel;

    private int currentEnemyIndex;
    private GameObject currentEnemy;
    private EnemyBase currentEnemyBase;

    private bool battleBusy;
    private bool changingEnemy;
    private bool gameEnded;

    private void Start()
    {
        Time.timeScale = 1f;

        if (winningPanel != null)
            winningPanel.SetActive(false);

        if (loosePanel != null)
            loosePanel.SetActive(false);

        currentEnemyIndex = 0;

        if (cameraFollow != null)
        {
            cameraFollow.StopFollowing();
        }

        StartCoroutine(StartBattle());
    }

    private void Update()
    {
        if (gameEnded)
            return;

        if (player == null)
            return;

        if (player.currentHearts <= 0)
        {
            LoseGame();
        }
    }

    private IEnumerator StartBattle()
    {
        battleBusy = true;
        changingEnemy = true;

        RemoveQuiz();

        currentEnemy = SpawnCurrentEnemy();

        if (currentEnemy == null)
        {
            battleBusy = false;
            changingEnemy = false;
            yield break;
        }

        if (
            playerStopPoints == null ||
            playerStopPoints.Length == 0 ||
            playerStopPoints[0] == null
        )
        {
            if (cameraFollow != null)
            {
                cameraFollow.StartFollowing();
            }

            changingEnemy = false;
            battleBusy = false;

            yield return new WaitForSeconds(quizDelay);

            if (!gameEnded)
                SpawnQuiz();

            yield break;
        }

        yield return StartCoroutine(
            MovePlayerTo(
                playerStopPoints[0]
            )
        );

        if (cameraFollow != null)
        {
            cameraFollow.StartFollowing();
        }

        yield return new WaitForSeconds(
            quizDelay
        );

        changingEnemy = false;
        battleBusy = false;

        if (!gameEnded)
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
            return null;

        if (spawnPoint == null)
            return null;

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

        if (player != null)
        {
            currentEnemyBase.player =
                player.transform;
        }

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

        if (gameEnded)
            return;

        if (currentEnemyBase == null)
            return;

        if (currentEnemyBase.IsDead)
            return;

        if (player == null)
            return;

        if (player.IsDead)
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

        if (gameEnded)
        {
            battleBusy = false;
            yield break;
        }

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
            !gameEnded &&
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

        if (gameEnded)
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

        if (currentEnemy != null)
        {
            yield return StartCoroutine(
                FadeEnemy(currentEnemy)
            );
        }

        currentEnemy = null;
        currentEnemyBase = null;

        currentEnemyIndex++;

        if (
            enemyPrefabs == null ||
            currentEnemyIndex >=
            enemyPrefabs.Length
        )
        {
            WinGame();
            yield break;
        }

        GameObject nextEnemy =
            SpawnCurrentEnemy();

        if (nextEnemy == null)
        {
            battleBusy = false;
            changingEnemy = false;
            yield break;
        }

        if (
            playerStopPoints == null ||
            currentEnemyIndex >=
            playerStopPoints.Length
        )
        {
            battleBusy = false;
            changingEnemy = false;
            yield break;
        }

        Transform playerStopPoint =
            playerStopPoints[currentEnemyIndex];

        if (playerStopPoint == null)
        {
            battleBusy = false;
            changingEnemy = false;
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

        if (!gameEnded)
            SpawnQuiz();

        battleBusy = false;
    }

    private IEnumerator FadeEnemy(
        GameObject enemy
    )
    {
        if (enemy == null)
            yield break;

        SpriteRenderer[] renderers =
            enemy.GetComponentsInChildren<SpriteRenderer>();

        if (
            renderers == null ||
            renderers.Length == 0
        )
        {
            Destroy(enemy);
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

        while (
            timer < enemyFadeDuration
        )
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    enemyFadeDuration
                );

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            for (
                int i = 0;
                i < renderers.Length;
                i++
            )
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

        for (
            int i = 0;
            i < renderers.Length;
            i++
        )
        {
            if (renderers[i] == null)
                continue;

            Color color =
                originalColors[i];

            color.a = 0f;

            renderers[i].color =
                color;
        }

        Destroy(enemy);
    }

    private IEnumerator MovePlayerTo(
        Transform target
    )
    {
        if (player == null)
            yield break;

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

        if (gameEnded)
            return;

        if (currentEnemyBase == null)
            return;

        if (currentEnemyBase.IsDead)
            return;

        if (player == null)
            return;

        if (player.IsDead)
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

        if (gameEnded)
        {
            battleBusy = false;
            yield break;
        }

        if (
            player != null &&
            player.currentHearts <= 0
        )
        {
            LoseGame();
            yield break;
        }

        RemoveQuiz();

        yield return new WaitForSeconds(
            quizDelay
        );

        if (
            !changingEnemy &&
            !gameEnded &&
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

        if (gameEnded)
            return;

        if (currentEnemyBase == null)
            return;

        if (currentEnemyBase.IsDead)
            return;

        if (player == null)
            return;

        if (player.IsDead)
            return;

        quizManager.SpawnRandomQuiz();
    }

    private void RemoveQuiz()
    {
        if (quizManager == null)
            return;

        quizManager.RemoveQuiz();
    }

    public void WinGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        battleBusy = true;
        changingEnemy = true;

        RemoveQuiz();

        if (winningPanel != null)
            winningPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void LoseGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        battleBusy = true;
        changingEnemy = true;

        RemoveQuiz();

        if (loosePanel != null)
            loosePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}