using System.Collections.Generic;
using UnityEngine;

public class BattleItemCollector : MonoBehaviour
{
    public static BattleItemCollector Instance;

    [SerializeField]
    private List<ItemRewardData> collectedItems = new List<ItemRewardData>();

    public int ItemCount => collectedItems.Count;

    private void Awake()
    {
        Instance = this;
    }

    public void CollectItem(string itemName, Sprite itemIcon)
    {
        if (string.IsNullOrEmpty(itemName))
            return;

        if (HasItem(itemName))
            return;

        ItemRewardData newItem = new ItemRewardData
        {
            itemName = itemName,
            itemIcon = itemIcon
        };

        collectedItems.Add(newItem);
    }

    public bool HasItem(string itemName)
    {
        for (int i = 0; i < collectedItems.Count; i++)
        {
            if (collectedItems[i].itemName == itemName)
                return true;
        }

        return false;
    }

    public List<ItemRewardData> GetCollectedItems()
    {
        return collectedItems;
    }

    public void ClearItems()
    {
        collectedItems.Clear();
    }
}

[System.Serializable]
public class ItemRewardData
{
    public string itemName;
    public Sprite itemIcon;
}