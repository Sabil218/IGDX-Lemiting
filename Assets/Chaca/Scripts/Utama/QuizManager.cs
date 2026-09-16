using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Prefabs")]
    public GameObject[] quizPrefabs;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    private List<GameObject> availableQuiz = new List<GameObject>();
    private GameObject currentQuiz;

    private bool quizActive;

    private void Awake()
    {
        availableQuiz.AddRange(quizPrefabs);
        quizActive = false;
    }

    public void SpawnRandomQuiz()
    {
        if (quizActive)
        {
            return;
        }

        if (availableQuiz.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(
            0,
            availableQuiz.Count
        );

        GameObject quizPrefab =
            availableQuiz[randomIndex];

        if (quizPrefab == null)
        {
            availableQuiz.RemoveAt(randomIndex);
            return;
        }

        currentQuiz = Instantiate(
            quizPrefab,
            spawnPoint.position,
            Quaternion.identity,
            spawnPoint
        );

        availableQuiz.RemoveAt(randomIndex);

        quizActive = true;
    }

    public void RemoveQuiz()
    {
        if (currentQuiz != null)
        {
            Destroy(currentQuiz);
            currentQuiz = null;
        }

        quizActive = false;
    }

    public void StopQuiz()
    {
        if (currentQuiz != null)
        {
            Destroy(currentQuiz);
            currentQuiz = null;
        }

        quizActive = false;
    }

    public bool HasQuizRemaining()
    {
        return availableQuiz.Count > 0;
    }

    public bool IsQuizActive()
    {
        return quizActive;
    }

    public void ResetQuiz()
    {
        if (currentQuiz != null)
        {
            Destroy(currentQuiz);
            currentQuiz = null;
        }

        availableQuiz.Clear();
        availableQuiz.AddRange(quizPrefabs);

        quizActive = false;
    }
}