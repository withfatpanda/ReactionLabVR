using UnityEngine;

// Attach this to the tip of a liquid-pouring prefab (e.g., bottle mouth).
// When the bottle pours into a Container (like a beaker),
// it will register its IngredientType once.
public class LiquidIngredientTag : MonoBehaviour
{
    [Header("Ingredient Type of this liquid")]
    public IngredientType ingredientType;

    private void OnTriggerEnter(Collider other)
    {
        Container container = other.GetComponentInParent<Container>();
        if (container != null)
        {
            // Register this ingredient if it's not already inside
            container.RegisterIngredient(ingredientType);
        }
    }
}