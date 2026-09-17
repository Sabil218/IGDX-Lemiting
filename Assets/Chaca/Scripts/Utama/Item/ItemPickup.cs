using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Ingredient")]
    public string ingredientName;
    public Sprite ingredientUISprite;

    [Header("Database")]
    public IngredientDatabase ingredientDatabase;

    private NewIngredientPopup newIngredientPopup;

    private bool pickedUp;

    private void Start()
    {
        newIngredientPopup = FindFirstObjectByType<NewIngredientPopup>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp)
            return;

        if (!other.CompareTag("Player"))
            return;

        pickedUp = true;

        bool alreadyCollected =
            ingredientDatabase.IsCollected(ingredientName);

        GameObject item = transform.root.gameObject;

        if (alreadyCollected)
        {
            Destroy(item);
            return;
        }

        ingredientDatabase.CollectIngredient(ingredientName);

        if (newIngredientPopup != null)
        {
            newIngredientPopup.ShowAfterItemDisappear(
                item,
                ingredientName,
                ingredientUISprite
            );
        }
        else
        {
            Destroy(item);
        }
    }
}