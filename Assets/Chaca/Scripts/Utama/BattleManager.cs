using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Player")]
    public Player player;

    [Header("Enemy")]
    public GameObject[] enemyPrefabs;
    public Transform[] enemySpawnPoints;

    [Header("Player Stop Points")]
    public Transform[] playerStopPoints;

    [Header("Quiz")]
    public QuizManager quizManager;
    public float quizDelay = 1f;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("UI")]
    public GameObject winningPanel;
    public GameObject loosePanel;

    [Header("Camera")]
    public CameraFollow cameraFollow;

    [Header("Win Trigger")]
    public GameObject winTrigger;

    private int currentEnemyIndex;
    private GameObject currentEnemy;
    private EnemyBase currentEnemyBase;

    private bool battleBusy;
    private bool changingEnemy;
    private bool gameEnded;
    private bool finalWalkStarted;

    private int battleID;

    private void Start()
    {
        Time.timeScale = 1f;

        battleBusy = true;
        changingEnemy = false;
        gameEnded = false;
        finalWalkStarted = false;

        battleID = 0;

        if (winningPanel != null)
        {
            winningPanel.SetActive(false);
        }

        if (loosePanel != null)
        {
            loosePanel.SetActive(false);
        }

        if (winTrigger != null)
        {
            winTrigger.SetActive(true);
        }

        if (cameraFollow == null)
        {
            cameraFollow =
                FindObjectOfType<CameraFollow>();
        }

        if (cameraFollow != null)
        {
            cameraFollow.StopFollowing();
        }

        SpawnEnemy(0);

        StartCoroutine(
            MovePlayerToFirstPoint()
        );
    }

    public void SpawnEnemy(int index)
    {
        if (gameEnded)
            return;

        if (enemyPrefabs == null)
            return;

        if (index < 0 ||
            index >= enemyPrefabs.Length)
            return;

        if (enemyPrefabs[index] == null)
            return;

        if (enemySpawnPoints == null)
            return;

        if (index >= enemySpawnPoints.Length)
            return;

        if (enemySpawnPoints[index] == null)
            return;

        currentEnemyIndex = index;

        GameObject enemyObject =
            Instantiate(
                enemyPrefabs[index],
                enemySpawnPoints[index].position,
                Quaternion.identity
            );

        currentEnemy = enemyObject;

        currentEnemyBase =
            enemyObject.GetComponent<EnemyBase>();

        if (currentEnemyBase != null)
        {
            currentEnemyBase.battleManager = this;

            if (player != null)
            {
                currentEnemyBase.player =
                    player.transform;
            }
        }
    }

    private IEnumerator MovePlayerToFirstPoint()
    {
        battleBusy = true;

        if (cameraFollow != null)
        {
            cameraFollow.StopFollowing();
        }

        yield return StartCoroutine(
            MovePlayerToNextPoint(0)
        );

        if (gameEnded)
            yield break;

        if (player != null &&
            player.animator != null)
        {
            player.animator.SetBool(
                "isRun",
                false
            );
        }

        yield return new WaitForSeconds(
            quizDelay
        );

        if (gameEnded)
            yield break;

        if (currentEnemy == null)
            yield break;

        if (currentEnemyBase != null &&
            currentEnemyBase.IsDead)
        {
            yield break;
        }

        if (quizManager != null)
        {
            quizManager.SpawnRandomQuiz();
        }

        battleBusy = false;
    }

    public void PlayerAttack()
    {
        if (gameEnded)
            return;

        if (battleBusy)
            return;

        if (finalWalkStarted)
            return;

        if (player == null)
            return;

        if (currentEnemy == null)
            return;

        if (currentEnemyBase != null &&
            currentEnemyBase.IsDead)
        {
            return;
        }

        GameObject attackEnemy =
            currentEnemy;

        int attackBattleID =
            battleID;

        battleBusy = true;

        StartCoroutine(
            player.Attack(
                attackEnemy.transform
            )
        );

        StartCoroutine(
            WaitForPlayerAttack(
                attackEnemy,
                attackBattleID
            )
        );
    }

    private IEnumerator WaitForPlayerAttack(
        GameObject attackEnemy,
        int attackBattleID
    )
    {
        yield return new WaitForSeconds(
            quizDelay
        );

        if (gameEnded)
            yield break;

        if (attackBattleID != battleID)
            yield break;

        if (attackEnemy == null)
            yield break;

        if (currentEnemy != attackEnemy)
            yield break;

        if (currentEnemyBase != null &&
            currentEnemyBase.IsDead)
        {
            yield break;
        }

        if (quizManager != null)
        {
            quizManager.SpawnRandomQuiz();
        }

        battleBusy = false;
    }

    public void EnemyAttack()
    {
        if (gameEnded)
            return;

        if (battleBusy)
            return;

        if (finalWalkStarted)
            return;

        if (player == null)
            return;

        if (currentEnemy == null)
            return;

        if (currentEnemyBase != null &&
            currentEnemyBase.IsDead)
        {
            return;
        }

        GameObject attackEnemy =
            currentEnemy;

        int attackBattleID =
            battleID;

        battleBusy = true;

        if (currentEnemyBase != null)
        {
            currentEnemyBase.Attack();
        }

        StartCoroutine(
            WaitForEnemyAttack(
                attackEnemy,
                attackBattleID
            )
        );
    }

    private IEnumerator WaitForEnemyAttack(
        GameObject attackEnemy,
        int attackBattleID
    )
    {
        yield return new WaitForSeconds(
            quizDelay
        );

        if (gameEnded)
            yield break;

        if (attackBattleID != battleID)
            yield break;

        if (player == null)
            yield break;

        if (player.IsDead)
        {
            GameOver();
            yield break;
        }

        if (attackEnemy == null)
            yield break;

        if (currentEnemy != attackEnemy)
            yield break;

        if (currentEnemyBase != null &&
            currentEnemyBase.IsDead)
        {
            yield break;
        }

        if (quizManager != null)
        {
            quizManager.SpawnRandomQuiz();
        }

        battleBusy = false;
    }

    public void EnemyDefeated()
    {
        if (changingEnemy)
            return;

        if (gameEnded)
            return;

        battleID++;

        if (quizManager != null)
        {
            quizManager.StopQuiz();
        }

        changingEnemy = true;

        StartCoroutine(
            ChangeToNextEnemy()
        );
    }

    private IEnumerator ChangeToNextEnemy()
    {
        battleBusy = true;

        if (quizManager != null)
        {
            quizManager.StopQuiz();
        }

        currentEnemy = null;
        currentEnemyBase = null;

        yield return null;

        if (gameEnded)
            yield break;

        int nextIndex =
            currentEnemyIndex + 1;

        if (nextIndex >= enemyPrefabs.Length)
        {
            changingEnemy = false;

            StartFinalWalk();

            yield break;
        }

        if (cameraFollow != null)
        {
            cameraFollow.StartFollowing();
        }

        SpawnEnemy(nextIndex);

        yield return StartCoroutine(
            MovePlayerToNextPoint(nextIndex)
        );

        if (gameEnded)
            yield break;

        if (player != null &&
            player.animator != null)
        {
            player.animator.SetBool(
                "isRun",
                false
            );
        }

        changingEnemy = false;

        yield return new WaitForSeconds(
            quizDelay
        );

        if (gameEnded)
            yield break;

        if (currentEnemy == null)
            yield break;

        if (currentEnemyBase != null &&
            currentEnemyBase.IsDead)
        {
            yield break;
        }

        if (quizManager != null)
        {
            quizManager.SpawnRandomQuiz();
        }

        battleBusy = false;
    }

    private IEnumerator MovePlayerToNextPoint(
        int nextIndex
    )
    {
        if (player == null)
            yield break;

        if (playerStopPoints == null)
            yield break;

        if (nextIndex < 0 ||
            nextIndex >= playerStopPoints.Length)
            yield break;

        if (playerStopPoints[nextIndex] == null)
            yield break;

        Transform targetPoint =
            playerStopPoints[nextIndex];

        Vector3 targetPosition =
            new Vector3(
                targetPoint.position.x,
                player.transform.position.y,
                player.transform.position.z
            );

        float originalMoveSpeed =
            player.moveSpeed;

        player.moveSpeed =
            moveSpeed;

        GameObject temporaryTarget =
            new GameObject(
                "PlayerMoveTarget"
            );

        temporaryTarget.transform.position =
            targetPosition;

        yield return StartCoroutine(
            player.MoveTo(
                temporaryTarget.transform
            )
        );

        player.moveSpeed =
            originalMoveSpeed;

        player.transform.position =
            targetPosition;

        if (player.animator != null)
        {
            player.animator.SetBool(
                "isRun",
                false
            );
        }

        Destroy(temporaryTarget);
    }

    private void StartFinalWalk()
    {
        if (finalWalkStarted)
            return;

        if (gameEnded)
            return;

        finalWalkStarted = true;
        battleBusy = true;

        if (quizManager != null)
        {
            quizManager.StopQuiz();
        }

        if (cameraFollow != null)
        {
            cameraFollow.StartFollowing();
        }

        if (winTrigger != null)
        {
            winTrigger.SetActive(true);
        }

        StartCoroutine(
            FinalPlayerWalk()
        );
    }

    private IEnumerator FinalPlayerWalk()
    {
        if (player == null)
            yield break;

        if (player.animator != null)
        {
            player.animator.SetBool(
                "isRun",
                true
            );
        }

        while (!gameEnded &&
               finalWalkStarted)
        {
            Vector3 position =
                player.transform.position;

            position.x +=
                moveSpeed *
                Time.deltaTime;

            player.transform.position =
                position;

            yield return null;
        }

        if (player.animator != null)
        {
            player.animator.SetBool(
                "isRun",
                false
            );
        }
    }

    public void TriggerWin()
    {
        if (gameEnded)
            return;

        if (!finalWalkStarted)
            return;

        WinGame();
    }

    private void WinGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        battleBusy = true;
        finalWalkStarted = false;

        battleID++;

        if (cameraFollow != null)
        {
            cameraFollow.StopFollowing();
        }

        if (quizManager != null)
        {
            quizManager.StopQuiz();
        }

        if (player != null &&
            player.animator != null)
        {
            player.animator.SetBool(
                "isRun",
                false
            );
        }

        if (winTrigger != null)
        {
            winTrigger.SetActive(false);
        }

        if (winningPanel != null)
        {
            winningPanel.SetActive(true);
        }
    }

    public void GameOver()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        battleBusy = true;
        finalWalkStarted = false;

        battleID++;

        if (cameraFollow != null)
        {
            cameraFollow.StopFollowing();
        }

        if (quizManager != null)
        {
            quizManager.StopQuiz();
        }

        if (player != null &&
            player.animator != null)
        {
            player.animator.SetBool(
                "isRun",
                false
            );
        }

        if (loosePanel != null)
        {
            loosePanel.SetActive(true);
        }
    }

    public GameObject GetCurrentEnemy()
    {
        return currentEnemy;
    }

    public EnemyBase GetCurrentEnemyBase()
    {
        return currentEnemyBase;
    }

    public bool IsBattleBusy()
    {
        return battleBusy;
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }
}