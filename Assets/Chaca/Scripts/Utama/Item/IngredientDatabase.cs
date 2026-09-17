using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientDatabase", menuName = "Game/Ingredient Database")]
public class IngredientDatabase : ScriptableObject
{
    [System.Serializable]
    public class IngredientData
    {
        public string ingredientName;
        public bool collected;
    }

    public List<IngredientData> ingredients = new List<IngredientData>();

    public bool IsCollected(string ingredientName)
    {
        string searchName = ingredientName.Trim();

        IngredientData ingredient = ingredients.Find(
            x => x.ingredientName.Trim().Equals(
                searchName,
                System.StringComparison.OrdinalIgnoreCase
            )
        );

        if (ingredient == null)
            return false;

        return ingredient.collected;
    }

    public void CollectIngredient(string ingredientName)
    {
        string searchName = ingredientName.Trim();

        IngredientData ingredient = ingredients.Find(
            x => x.ingredientName.Trim().Equals(
                searchName,
                System.StringComparison.OrdinalIgnoreCase
            )
        );

        if (ingredient != null)
        {
            ingredient.collected = true;
        }
    }
}