using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Player")]
    public Player player;

    [Header("Heart UI")]
    public Image[] hearts;

    private void Start()
    {
        UpdateHearts();
    }

    public void UpdateHearts()
    {
        if (player == null)
            return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            hearts[i].gameObject.SetActive(i < player.currentHearts);
        }
    }
}