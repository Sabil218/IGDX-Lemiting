using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text itemName;

    public void SetReward(ItemRewardData data)
    {
        if (icon != null)
        {
            icon.sprite = data.itemIcon;
            icon.enabled = data.itemIcon != null;
        }

        if (itemName != null)
        {
            itemName.text = data.itemName;
        }
    }
}