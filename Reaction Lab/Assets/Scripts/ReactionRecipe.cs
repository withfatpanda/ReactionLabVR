
using UnityEngine;

// Represents a single chemical reaction setup used in Reaction Lab VR.
// This class is serializable and shared across all systems (machines, libraries).
[System.Serializable]
public class ReactionRecipe
{
    [Header("First Ingredient")]
    public IngredientType ingredientAType;

    [Header("Second Ingredient (None = Single)")]
    public IngredientType ingredientBType;

    [Header("Required Machine")]
    public MachineType requiredMachine;

    [Header("Final Compound")]
    public GameObject resultingCompoundPrefab;

    [Header("Single Ingredient Reaction?")]
    public bool allowSingleIngredient = false;
}
