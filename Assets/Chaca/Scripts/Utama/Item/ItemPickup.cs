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

        if (!alreadyCollected)
        {
            ingredientDatabase.CollectIngredient(ingredientName);

            if (newIngredientPopup != null)
            {
                newIngredientPopup.ShowIngredient(
                    ingredientName,
                    ingredientUISprite
                );
            }
        }

        Destroy(transform.root.gameObject);
    }
}