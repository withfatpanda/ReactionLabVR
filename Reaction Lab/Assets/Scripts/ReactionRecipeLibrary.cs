using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central place to store all possible chemical reactions in the game.
/// Attach this to a single GameObject in the scene (e.g., "GameManager").
/// Machines will query this list based on their MachineType.
/// </summary>
public class ReactionRecipeLibrary : MonoBehaviour
{
    [Header("All available reactions in the game")]
    public List<ReactionRecipe> allRecipes = new List<ReactionRecipe>();
}