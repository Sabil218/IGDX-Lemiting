using UnityEngine;

public class WinPanelReward : MonoBehaviour
{
    public Transform rewardContainer;
    public GameObject rewardItemPrefab;

    private void OnEnable()
    {
        ShowRewards();
    }

    private void ShowRewards()
    {
        ClearRewards();

        if (BattleItemCollector.Instance == null)
            return;

        var rewards =
            BattleItemCollector.Instance.GetCollectedItems();

        for (int i = 0; i < rewards.Count; i++)
        {
            GameObject rewardObject =
                Instantiate(
                    rewardItemPrefab,
                    rewardContainer
                );

            RewardItemUI rewardUI =
                rewardObject.GetComponent<RewardItemUI>();

            if (rewardUI != null)
            {
                rewardUI.SetReward(rewards[i]);
            }
        }
    }

    private void ClearRewards()
    {
        if (rewardContainer == null)
            return;

        for (int i = rewardContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(rewardContainer.GetChild(i).gameObject);
        }
    }
}