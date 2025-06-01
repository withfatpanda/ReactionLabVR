using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles machine logic: triggers reactions based on ingredients inside Beakers
// or input zones (like Distiller). Pulls valid recipes from a shared ReactionRecipeLibrary.
public class MachineReactor : MonoBehaviour
{
    [Header("Machine Type")]
    public MachineType machineType; // Set per prefab

    [Header("Reaction Recipe Library")]
    public ReactionRecipeLibrary recipeLibrary;

    [Header("Activation Settings")]
    public Collider activationCollider;
    public bool testReactInEditor = false;

    [Header("Distiller Input Zone")]
    public Transform inputTriggerArea;
    private List<GameObject> inputIngredients = new List<GameObject>();

    [Header("Distiller Output")]
    public Transform outputSpawnPoint;

    private Container activeContainer = null;

    void Update()
    {
        if (testReactInEditor)
        {
            testReactInEditor = false;
            TryReact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activationCollider != null && other == activationCollider)
        {
            TryReact();
            return;
        }

        if (machineType == MachineType.Distiller && inputTriggerArea != null)
        {
            Ingredient ingredient = other.GetComponent<Ingredient>();
            if (ingredient != null && other.transform.IsChildOf(inputTriggerArea))
            {
                if (!inputIngredients.Contains(other.gameObject))
                    inputIngredients.Add(other.gameObject);
            }
        }
        else
        {
            Container container = other.GetComponent<Container>();
            if (container != null)
            {
                activeContainer = container;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (machineType == MachineType.Distiller && inputTriggerArea != null)
        {
            if (inputIngredients.Contains(other.gameObject))
                inputIngredients.Remove(other.gameObject);
        }
        else
        {
            Container container = other.GetComponent<Container>();
            if (container != null && container == activeContainer)
            {
                activeContainer = null;
            }
        }
    }

    public void TryReact()
    {
        if (recipeLibrary == null)
        {
            Debug.LogWarning("No recipe library assigned to MachineReactor.");
            return;
        }

        if (machineType == MachineType.Distiller)
        {
            TryDistill();
        }
        else
        {
            TryNormalReaction();
        }
    }

    private void TryNormalReaction()
    {
        if (activeContainer == null || !activeContainer.HasContents())
        {
            Debug.Log("No Beaker or no ingredients inside.");
            return;
        }

        List<IngredientType> contents = activeContainer.GetContainedIngredientTypes();

        foreach (ReactionRecipe recipe in recipeLibrary.allRecipes)
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

    private IEnumerator ProcessNormalReaction(Container container, ReactionRecipe recipe)
    {
        Debug.Log("Processing normal machine reaction...");
        yield return new WaitForSeconds(2f);
        container.SetContents(recipe.resultingCompoundPrefab);
        Debug.Log($"Beaker now holds: {recipe.resultingCompoundPrefab.name}");
    }

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
                collectedTypes.Add(ing.ingredientType);
        }

        foreach (ReactionRecipe recipe in recipeLibrary.allRecipes)
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

    private IEnumerator ProcessDistillation(ReactionRecipe recipe)
    {
        Debug.Log("Processing distillation...");
        yield return new WaitForSeconds(2f);

        foreach (GameObject obj in inputIngredients)
        {
            if (obj != null)
                Destroy(obj);
        }

        inputIngredients.Clear();

        if (outputSpawnPoint != null && recipe.resultingCompoundPrefab != null)
        {
            Instantiate(recipe.resultingCompoundPrefab, outputSpawnPoint.position, Quaternion.identity);
            Debug.Log($"Output spawned: {recipe.resultingCompoundPrefab.name}");
        }
    }
}