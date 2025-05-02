using UnityEngine;

// Defines a recipe that tells the game which ingredients + machine produce a compound

[System.Serializable]
public class ReactionRecipe
{
    [Header("Ingredients Needed")]
    public IngredientType ingredientAType; // First ingredient required
    public IngredientType ingredientBType; // Optional second ingredient (can be None)

    [Header("Machine Type")]
    public MachineType requiredMachine; // Which machine is needed to perform this reaction

    [Header("Resulting Compound")]
    public GameObject resultingCompoundPrefab; // The prefab of the resulting compound
}