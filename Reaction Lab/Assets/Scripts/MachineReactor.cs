
using System.Collections.Generic;
using UnityEngine;

// This script manages the interaction between chemical containers and lab machines.
// It reads from the central ReactionRecipeLibrary to determine if a reaction should occur
// when a container is inserted and a machine is activated. Handles unique cases for 
// machines like the Distiller and Electric Circuit, which have special logic.

public class MachineReactor : MonoBehaviour
{
    // The type of machine this reactor represents (e.g., BunsenBurner, Freezer, etc.)
    [Header("Machine Type")]
    public MachineType machineType;

    // Reference to the reaction recipe library which contains all reaction rules
    [Header("Recipe Library")]
    public ReactionRecipeLibrary recipeLibrary;

    // Used only by Distiller machines: the input container permanently inside the machine
    [Header("Distiller Only")]
    public Container fixedInputContainer;

    // The Transform where a result will be spawned if no output container is present (Distiller only)
    public Transform outputZone;

    // For Electric Circuit only: the required electrodes that must be placed into the container
    [Header("Electric Circuit Only")]
    public GameObject electrodeA;
    public GameObject electrodeB;

    // Internal list tracking which containers are currently in or on the machine
    private List<Container> currentContainers = new List<Container>();

    private void Start()
    {
        // On start, detect containers already in the machine's area using a physics overlap check
        Collider[] overlapping = Physics.OverlapBox(transform.position, transform.localScale * 1.5f);
        foreach (var col in overlapping)
        {
            Container c = col.GetComponent<Container>();
            if (c != null && !currentContainers.Contains(c))
            {
                currentContainers.Add(c);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // When a container enters the trigger area, register it
        Container container = other.GetComponent<Container>();
        if (container != null && !currentContainers.Contains(container))
        {
            currentContainers.Add(container);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When a container exits the trigger area, unregister it
        Container container = other.GetComponent<Container>();
        if (container != null)
        {
            currentContainers.Remove(container);
        }
    }

    // This method should be called externally when the machine is activated (e.g., a button is pressed)
    public void ActivateReaction()
    {
        TryReact();
    }

    // This method determines if a valid reaction should occur based on the current state
    private void TryReact()
    {
        // Handle Distiller-specific logic
        if (machineType == MachineType.Distiller)
        {
            TryDistillerReaction();
            return;
        }

        // For all other machines, iterate through tracked containers
        foreach (Container container in currentContainers)
        {
            if (!container.HasContents()) continue;

            // Special case for Electric Circuit: must have both electrodes inside
            if (machineType == MachineType.ElectricCircuit &&
               (!IsElectrodeInside(container, electrodeA) || !IsElectrodeInside(container, electrodeB)))
            {
                Debug.Log("Electrodes not properly inserted into container.");
                continue;
            }

            // Try matching container contents to a reaction
            AttemptMatch(container);
        }
    }

    // Special logic for Distiller machines which take input from a fixed container
    // and output to another container or into the world
    private void TryDistillerReaction()
    {
        if (fixedInputContainer == null || !fixedInputContainer.HasContents()) return;

        // Get current ingredients from the fixed input container
        List<IngredientType> ingredients = fixedInputContainer.GetContainedIngredientTypes();

        foreach (ReactionRecipe recipe in recipeLibrary.allRecipes)
        {
            // Skip recipes not intended for the Distiller
            if (recipe.requiredMachine != MachineType.Distiller) continue;

            // Check if this recipe matches the container's contents
            if (IngredientsMatch(recipe, ingredients))
            {
                GameObject result = recipe.resultingCompoundPrefab;

                // Clear the fixed container's contents after reaction
                fixedInputContainer.ClearContents();

                // Try to place the result into a secondary container (beneath the distiller)
                foreach (Container target in currentContainers)
                {
                    if (target == fixedInputContainer) continue;

                    target.SetContents(result);
                    Debug.Log("Distiller output sent to container below.");
                    return;
                }

                // If no container below, spawn the result into the world
                if (outputZone != null && result != null)
                {
                    Instantiate(result, outputZone.position, Quaternion.identity);
                    Debug.Log("Distiller output spawned into world.");
                }
                return;
            }
        }

        // If no match was found
        Debug.Log("No valid distillation recipe.");
    }

    // Tries to match a container's contents to any valid recipe for this machine type
    private void AttemptMatch(Container container)
    {
        List<IngredientType> ingredients = container.GetContainedIngredientTypes();

        foreach (ReactionRecipe recipe in recipeLibrary.allRecipes)
        {
            // Skip recipes meant for other machines
            if (recipe.requiredMachine != machineType) continue;

            // Check for match and apply result
            if (IngredientsMatch(recipe, ingredients))
            {
                container.SetContents(recipe.resultingCompoundPrefab);
                Debug.Log("Reaction successful: " + recipe.resultingCompoundPrefab.name);
                return;
            }
        }

        Debug.Log("No matching recipe for container on " + machineType);
    }

    // Determines whether the given ingredients match the recipe requirements
    private bool IngredientsMatch(ReactionRecipe recipe, List<IngredientType> ingredients)
    {
        // Handle single-ingredient recipes (when ingredientBType is None)
        if (recipe.allowSingleIngredient &&
            recipe.ingredientBType == IngredientType.None &&
            ingredients.Contains(recipe.ingredientAType))
        {
            return true;
        }

        // Standard dual-ingredient or double-of-same-ingredient reaction
        return (ingredients.Contains(recipe.ingredientAType) &&
                ingredients.Contains(recipe.ingredientBType)) ||
               (recipe.ingredientAType == recipe.ingredientBType &&
                ingredients.FindAll(i => i == recipe.ingredientAType).Count >= 2);
    }

    // Used by Electric Circuit to check whether both electrodes are inside the container
    private bool IsElectrodeInside(Container container, GameObject electrode)
    {
        Collider containerCollider = container.GetComponent<Collider>();
        Collider electrodeCollider = electrode.GetComponent<Collider>();

        if (containerCollider == null || electrodeCollider == null) return false;
        return containerCollider.bounds.Intersects(electrodeCollider.bounds);
    }
}
