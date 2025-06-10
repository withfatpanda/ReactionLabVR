using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls machine behavior in Reaction Lab VR.
// It supports reactions triggered by buttons, freezer door closes, and special logic
// for distillers and electric circuits. It also logs successful reactions to the clipboard.

public class MachineReactor : MonoBehaviour
{
    [Header("Machine Type")]
    public MachineType machineType;

    [Header("Reaction Configuration")]
    public ReactionRecipeLibrary recipeLibrary;

    [Header("Trigger Zones")]
    public Collider containerTriggerZone;  // Detects containers placed into standard machines
    public Collider buttonTriggerZone;     // Button or lever that triggers the reaction (not used by Freezer)

    [Header("Distiller Only")]
    public Container fixedInputContainer;  // For distillers: internal container permanently attached
    public Transform outputZone;           // Where the result spawns if no output container is found

    [Header("Electric Circuit Only")]
    public GameObject electrodeA;          // Required rod A
    public GameObject electrodeB;          // Required rod B

    [Header("Clipboard Display")]
    public RecipeClipboardUI clipboardUI;  // Reference to clipboard UI to log reactions

    // Tracks active containers in standard machines (ignored by Distiller)
    private List<Container> currentContainers = new List<Container>();

    private void OnTriggerEnter(Collider other)
    {
        // Add a container if it enters the container trigger zone
        if (containerTriggerZone != null && other == containerTriggerZone)
        {
            Container c = other.GetComponent<Container>();
            if (c != null && !currentContainers.Contains(c))
            {
                currentContainers.Add(c);
            }
        }

        // Activate reaction if button is pressed
        if (buttonTriggerZone != null && other == buttonTriggerZone)
        {
            ActivateReaction();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove container if it exits the container trigger zone
        if (containerTriggerZone != null && other == containerTriggerZone)
        {
            Container c = other.GetComponent<Container>();
            if (c != null)
            {
                currentContainers.Remove(c);
            }
        }
    }

    // Freezer closes and automatically triggers reaction
    public void TriggerFreezerReaction()
    {
        if (machineType == MachineType.Freezer)
        {
            Debug.Log("Freezer closed, attempting cold reaction...");
            ActivateReaction();
        }
    }

    // Determines what type of reaction logic to use
    public void ActivateReaction()
    {
        if (machineType == MachineType.Distiller)
        {
            TryDistillerReaction();
        }
        else
        {
            TryStandardReactions();
        }
    }

    // Handles typical container-based machines (Beaker, Bunsen Burner, Freezer)
    private void TryStandardReactions()
    {
        foreach (Container container in currentContainers)
        {
            if (!container.HasContents()) continue;

            // Electric circuit needs both electrodes to be inside the container
            if (machineType == MachineType.ElectricCircuit &&
                (!IsElectrodeInside(container, electrodeA) || !IsElectrodeInside(container, electrodeB)))
            {
                Debug.Log("Electrodes not properly inserted into container.");
                continue;
            }

            AttemptMatch(container);
        }
    }

    // Handles the fixed distiller logic using the internal container
    private void TryDistillerReaction()
    {
        if (fixedInputContainer == null || !fixedInputContainer.HasContents()) return;

        List<IngredientType> ingredients = fixedInputContainer.GetContainedIngredientTypes();

        foreach (ReactionRecipe recipe in recipeLibrary.allRecipes)
        {
            if (recipe.requiredMachine != MachineType.Distiller) continue;
            if (!IngredientsMatch(recipe, ingredients)) continue;

            GameObject result = recipe.resultingCompoundPrefab;

            // Clear the internal distiller container
            fixedInputContainer.ClearContents();

            // Try to output into a separate container
            foreach (Container target in currentContainers)
            {
                if (target == fixedInputContainer) continue;

                target.SetContents(result);
                Debug.Log("Distiller output sent to container below.");

                // Log to clipboard if available
                if (clipboardUI != null)
                {
                    clipboardUI.AddReactionToClipboard(
                        recipe.ingredientAType.ToString(),
                        recipe.ingredientBType.ToString(),
                        result.name,
                        machineType.ToString()
                    );
                }

                return;
            }

            // Fallback: spawn result directly into the world
            if (outputZone != null && result != null)
            {
                Instantiate(result, outputZone.position, Quaternion.identity);
                Debug.Log("Distiller output spawned into world.");

                if (clipboardUI != null)
                {
                    clipboardUI.AddReactionToClipboard(
                        recipe.ingredientAType.ToString(),
                        recipe.ingredientBType.ToString(),
                        result.name,
                        machineType.ToString()
                    );
                }
            }

            return;
        }

        Debug.Log("No valid distillation recipe.");
    }

    // Attempts to match container contents to known recipes
    private void AttemptMatch(Container container)
    {
        List<IngredientType> ingredients = container.GetContainedIngredientTypes();

        foreach (ReactionRecipe recipe in recipeLibrary.allRecipes)
        {
            if (recipe.requiredMachine != machineType) continue;
            if (!IngredientsMatch(recipe, ingredients)) continue;

            container.SetContents(recipe.resultingCompoundPrefab);
            Debug.Log("Reaction successful: " + recipe.resultingCompoundPrefab.name);

            // Log to clipboard if available
            if (clipboardUI != null)
            {
                clipboardUI.AddReactionToClipboard(
                    recipe.ingredientAType.ToString(),
                    recipe.ingredientBType.ToString(),
                    recipe.resultingCompoundPrefab.name,
                    machineType.ToString()
                );
            }

            return;
        }

        Debug.Log("No matching recipe for container on " + machineType);
    }

    // Ingredient matcher for 1 or 2 ingredient recipes
    private bool IngredientsMatch(ReactionRecipe recipe, List<IngredientType> ingredients)
    {
        if (recipe.allowSingleIngredient &&
            recipe.ingredientBType == IngredientType.None &&
            ingredients.Contains(recipe.ingredientAType))
        {
            return true;
        }

        return (ingredients.Contains(recipe.ingredientAType) &&
                ingredients.Contains(recipe.ingredientBType)) ||
               (recipe.ingredientAType == recipe.ingredientBType &&
                ingredients.FindAll(i => i == recipe.ingredientAType).Count >= 2);
    }

    // For electric circuit: check if the electrode objects intersect the container's collider
    private bool IsElectrodeInside(Container container, GameObject electrode)
    {
        Collider containerCollider = container.GetComponent<Collider>();
        Collider electrodeCollider = electrode.GetComponent<Collider>();

        if (containerCollider == null || electrodeCollider == null) return false;
        return containerCollider.bounds.Intersects(electrodeCollider.bounds);
    }
}
