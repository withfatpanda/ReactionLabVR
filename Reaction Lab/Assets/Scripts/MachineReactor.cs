using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script handles all machines like Stirrer, Burner, Freezer, Centrifuge, Distiller, and Electric Circuit.
// Reactions happen based on ingredients inside a Beaker (Container) placed inside the machine.
// It also allows manual testing in the Unity Editor without VR headset.

public class MachineReactor : MonoBehaviour
{
    [Header("Machine Settings")]
    public MachineType machineType; // Select which type of machine this is
    public List<ReactionRecipe> availableReactions = new List<ReactionRecipe>(); // List of recipes this machine can perform

    [Header("Activation Settings")]
    public Collider activationCollider; // Assign a specific button or knob collider to trigger reaction by touch

    [Header("Testing (Editor Only)")]
    public bool testReactInEditor = false; // If set to true during Play Mode, will trigger TryReact() immediately (for desktop testing)

    [Header("Input Settings (Distiller ONLY)")]
    public Transform inputTriggerArea; // Funnel area for pouring into Distillers only
    private List<GameObject> inputIngredients = new List<GameObject>(); // Ingredients poured into Distiller

    [Header("Output Settings (Distiller ONLY)")]
    public Transform outputSpawnPoint; // Spout where distilled compounds spawn

    // Tracks the currently detected Beaker (Container) inside the machine trigger zone
    private Container activeContainer = null;

    private void Update()
    {
        // Check if manual test activation was triggered during Play Mode
        if (testReactInEditor)
        {
            testReactInEditor = false; // Reset immediately so it only happens once
            TryReact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If collision happens with the Activation Collider, trigger reaction immediately
        if (activationCollider != null && other == activationCollider)
        {
            TryReact();
            return;
        }

        // Handle special case for Distillers: detect poured ingredients into input funnel
        if (machineType == MachineType.Distiller && inputTriggerArea != null)
        {
            Ingredient ingredient = other.GetComponent<Ingredient>();
            if (ingredient != null && other.transform.IsChildOf(inputTriggerArea))
            {
                if (!inputIngredients.Contains(other.gameObject))
                {
                    inputIngredients.Add(other.gameObject);
                }
            }
        }
        else
        {
            // Handle normal machines: detect Beakers (Container.cs) placed inside machine zone
            Container container = other.GetComponent<Container>();
            if (container != null)
            {
                activeContainer = container;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Handle ingredients exiting Distiller input funnel
        if (machineType == MachineType.Distiller && inputTriggerArea != null)
        {
            if (inputIngredients.Contains(other.gameObject))
            {
                inputIngredients.Remove(other.gameObject);
            }
        }
        else
        {
            // Handle Beakers exiting normal machines
            Container container = other.GetComponent<Container>();
            if (container != null && container == activeContainer)
            {
                activeContainer = null;
            }
        }
    }

    // Public method to try to process a reaction.
    // Called either manually (testing), or automatically (collider touch).
    public void TryReact()
    {
        if (machineType == MachineType.Distiller)
        {
            TryDistill();
        }
        else
        {
            TryNormalReaction();
        }
    }

    // Handles normal machine reactions (Stirrer, Burner, Freezer, Centrifuge, Electric Circuit).
    private void TryNormalReaction()
    {
        if (activeContainer == null || !activeContainer.HasContents())
        {
            Debug.Log("No Beaker or no ingredients inside.");
            return;
        }

        List<IngredientType> contents = activeContainer.GetContainedIngredientTypes();

        foreach (ReactionRecipe recipe in availableReactions)
        {
            if (machineType != recipe.requiredMachine)
                continue;

            bool hasA = contents.Contains(recipe.ingredientAType);
            bool hasB = contents.Contains(recipe.ingredientBType);

            if (hasA && (recipe.ingredientBType == 0 || hasB))
            {
                StartCoroutine(ProcessNormalReaction(activeContainer, recipe));
                return;
            }
        }

        Debug.Log("No valid reaction found inside Beaker.");
    }

    // Coroutine to process reaction after delay (normal machines).
    private IEnumerator ProcessNormalReaction(Container container, ReactionRecipe recipe)
    {
        Debug.Log("Processing normal machine reaction...");

        yield return new WaitForSeconds(2f); // Simulated reaction time delay

        container.SetContents(recipe.resultingCompoundPrefab);

        Debug.Log($"Beaker now holds: {recipe.resultingCompoundPrefab.name}");
    }

    // Handles special Distiller reactions (pour input, spawn output).
    private void TryDistill()
    {
        if (inputIngredients.Count == 0)
        {
            Debug.Log("No ingredients poured into Distiller input.");
            return;
        }

        List<IngredientType> collectedTypes = new List<IngredientType>();

        foreach (GameObject obj in inputIngredients)
        {
            Ingredient ing = obj.GetComponent<Ingredient>();
            if (ing != null && !collectedTypes.Contains(ing.ingredientType))
            {
                collectedTypes.Add(ing.ingredientType);
            }
        }

        foreach (ReactionRecipe recipe in availableReactions)
        {
            if (machineType != recipe.requiredMachine)
                continue;

            bool hasA = collectedTypes.Contains(recipe.ingredientAType);
            bool hasB = collectedTypes.Contains(recipe.ingredientBType);

            if (hasA && (recipe.ingredientBType == 0 || hasB))
            {
                StartCoroutine(ProcessDistillation(recipe));
                return;
            }
        }

        Debug.Log("No valid distillation reaction found.");
    }

    // Coroutine to process distillation after delay.
    private IEnumerator ProcessDistillation(ReactionRecipe recipe)
    {
        Debug.Log("Processing distillation...");

        yield return new WaitForSeconds(2f); // Simulated distillation time delay

        foreach (GameObject obj in inputIngredients)
        {
            if (obj != null)
            {
                Destroy(obj); // Destroy poured ingredients after reaction
            }
        }
        inputIngredients.Clear();

        // Spawn resulting compound at output spout
        if (outputSpawnPoint != null && recipe.resultingCompoundPrefab != null)
        {
            Instantiate(recipe.resultingCompoundPrefab, outputSpawnPoint.position, Quaternion.identity);
            Debug.Log($"Output spawned: {recipe.resultingCompoundPrefab.name}");
        }
    }
}
