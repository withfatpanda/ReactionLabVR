using UnityEngine;

// This script is attached to any ingredient or compound prefab
// It defines what type of ingredient this object represents

public class Ingredient : MonoBehaviour
{
    [Header("Ingredient Type")]
    public IngredientType ingredientType; // Select from the dropdown (Raw Ingredients and Compounds)
}