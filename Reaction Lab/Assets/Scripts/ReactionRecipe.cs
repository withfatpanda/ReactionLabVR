using UnityEngine;

/// <summary>
/// Represents a single chemical reaction setup used in Reaction Lab VR.
/// This class is serializable and shared across all systems (machines, libraries).
/// </summary>
[System.Serializable]
public class ReactionRecipe
{
    [Header("First required ingredient (dropdown)")]
    public IngredientType ingredientAType;

    [Header("Second required ingredient (optional)")]
    public IngredientType ingredientBType;

    [Header("Which machine performs this reaction")]
    public MachineType requiredMachine;

    [Header("Prefab result of the reaction")]
    public GameObject resultingCompoundPrefab;
}