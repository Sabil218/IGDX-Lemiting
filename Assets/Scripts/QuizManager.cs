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

    private void Awake()
    {
        availableQuiz.AddRange(quizPrefabs);
    }

    private void Start()
    {
        SpawnRandomQuiz();
    }

    public void SpawnRandomQuiz()
    {
        if (availableQuiz.Count == 0)
        {
            Debug.Log("Semua quiz sudah dipakai.");
            return;
        }

        if (currentQuiz != null)
        {
            Destroy(currentQuiz);
        }

        int randomIndex = Random.Range(0, availableQuiz.Count);

        GameObject quizPrefab = availableQuiz[randomIndex];

        currentQuiz = Instantiate(
            quizPrefab,
            spawnPoint.position,
            Quaternion.identity,
            spawnPoint
        );

        availableQuiz.RemoveAt(randomIndex);
    }

    public void RemoveQuiz()
    {
        if (currentQuiz != null)
        {
            Destroy(currentQuiz);
            currentQuiz = null;
        }
    }

    public bool HasQuizRemaining()
    {
        return availableQuiz.Count > 0;
    }

    public void ResetQuiz()
    {
        availableQuiz.Clear();
        availableQuiz.AddRange(quizPrefabs);
    }
}