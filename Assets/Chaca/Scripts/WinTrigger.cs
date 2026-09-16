using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public BattleManager battleManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (battleManager != null)
            battleManager.TriggerWin();
    }
}