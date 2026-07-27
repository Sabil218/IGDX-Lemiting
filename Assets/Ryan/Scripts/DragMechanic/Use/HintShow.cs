using System.Collections;
using UnityEngine;
using TMPro;

public class HintShow : MonoBehaviour
{
    [SerializeField] private GameObject hintObject;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private float hintDuration = 5f;

    void Start()
    {
        hintObject.SetActive(false);
    }

    public void ShowHintFor5Seconds()
    {
        StartCoroutine(HintCoroutine());
    }

    private IEnumerator HintCoroutine()
    {
        //Get Current Word
        string correctWord = WordManager.instance.GetCurrentWord();
        hintText.SetText("Jawaban: " + correctWord);

        hintObject.SetActive(true);
        Debug.Log("Hint ON: Menampilkan kata " + correctWord);

        yield return new WaitForSeconds(hintDuration);

        hintObject.SetActive(false);
        Debug.Log("Hint OFF");
    }
}