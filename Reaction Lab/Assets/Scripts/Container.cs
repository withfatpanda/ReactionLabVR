using System.Collections.Generic;
using UnityEngine;

// Manages any container object (like Beakers or Test Tubes)
// Tracks which ingredient objects are physically inside using collision triggers

public class Container : MonoBehaviour
{
    [Header("Compound Spawnpoint")]
    public Transform contentsSpawnPoint; // Where new compounds will appear after reactions

    [Header("Ingredients")]
    public List<GameObject> currentIngredients = new List<GameObject>(); // List of GameObjects currently inside

    private void OnTriggerEnter(Collider other)
    {
        // Add ingredients entering the container
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient != null && !currentIngredients.Contains(other.gameObject))
        {
            currentIngredients.Add(other.gameObject);

            // Parent the ingredient to this container (optional, keeps hierarchy clean)
            other.transform.SetParent(this.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove ingredients exiting the container
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient != null && currentIngredients.Contains(other.gameObject))
        {
            currentIngredients.Remove(other.gameObject);

            // Unparent the ingredient so it returns to world space
            other.transform.SetParent(null);
        }
    }

    /// <summary>
    /// Returns true if this container currently holds any ingredients.
    /// </summary>
    public bool HasContents()
    {
        return currentIngredients.Count > 0;
    }

    /// <summary>
    /// Returns a list of IngredientTypes currently inside the container.
    /// </summary>
    public List<IngredientType> GetContainedIngredientTypes()
    {
        List<IngredientType> ingredientTypes = new List<IngredientType>();

        foreach (GameObject obj in currentIngredients)
        {
            Ingredient ingredient = obj.GetComponent<Ingredient>();
            if (ingredient != null)
            {
                if (!ingredientTypes.Contains(ingredient.ingredientType))
                {
                    ingredientTypes.Add(ingredient.ingredientType);
                }
            }
        }

        return ingredientTypes;
    }

    /// <summary>
    /// Clears the container's contents and spawns a new compound prefab inside it.
    /// </summary>
    public void SetContents(GameObject newCompoundPrefab)
    {
        // Destroy all current ingredients
        foreach (GameObject ingredientObj in currentIngredients)
        {
            if (ingredientObj != null)
            {
                Destroy(ingredientObj);
            }
        }
        currentIngredients.Clear();

        // Spawn new compound inside
        if (newCompoundPrefab != null && contentsSpawnPoint != null)
        {
            GameObject spawnedCompound = Instantiate(newCompoundPrefab, contentsSpawnPoint.position, Quaternion.identity);
            spawnedCompound.transform.SetParent(this.transform); // Parent the spawned compound to the container
        }
    }
}
