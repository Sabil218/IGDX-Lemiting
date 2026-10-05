using UnityEngine;

public class CookingUnlockTrigger : MonoBehaviour
{
    [Header("Cooking Manager")]
    public CookingManager cookingManager;

    [Header("Object yang punya fungsi Unlock Level")]
    public GameObject levelUnlockObject;

    [Header("Nama fungsi Unlock")]
    public string unlockMethodName = "UnlockNextLevel";

    private bool unlocked = false;

    private void Update()
    {
        if (unlocked)
            return;

        if (cookingManager == null)
            return;

        if (cookingManager.cookingSequence == null ||
            cookingManager.cookingSequence.Count == 0)
            return;

        // Cek phase terakhir
        CookingManager.CookingPhase lastPhase =
            cookingManager.cookingSequence[
                cookingManager.cookingSequence.Count - 1
            ];

        if (lastPhase.phaseContainer == null)
            return;

        // Kalau phase terakhir sudah tidak aktif,
        // berarti cooking sudah lanjut melewati phase terakhir
        if (!lastPhase.phaseContainer.activeSelf)
        {
            UnlockLevel();
        }
    }

    private void UnlockLevel()
    {
        if (levelUnlockObject == null)
        {
            Debug.LogWarning("Level Unlock Object belum diisi!");
            return;
        }

        unlocked = true;

        levelUnlockObject.SendMessage(
            unlockMethodName,
            SendMessageOptions.RequireReceiver
        );

        Debug.Log("Cooking selesai! Level berikutnya di-unlock.");
    }
}