
using System.Collections.Generic;
using UnityEngine;

// Manages the contents of a container like a beaker or test tube.
// Tracks ingredients (both solid GameObjects and poured liquid types),
// handles automatic parenting, and spawns resulting compounds.
public class Container : MonoBehaviour
{
    [Header("Ingredients")]
    public Transform contentsSpawnPoint;

    public List<GameObject> currentIngredients = new List<GameObject>();
    public List<IngredientType> containedIngredientTypes = new List<IngredientType>();

    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();

    private Dictionary<IngredientType, int> ingredientCounts = new Dictionary<IngredientType, int>();

    private void OnTriggerEnter(Collider other)
    {
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient != null && !trackedObjects.Contains(other.gameObject))
        {
            IngredientType type = ingredient.ingredientType;

            if (!containedIngredientTypes.Contains(type))
                containedIngredientTypes.Add(type);

            if (!ingredientCounts.ContainsKey(type))
                ingredientCounts[type] = 0;

            ingredientCounts[type]++;

            trackedObjects.Add(other.gameObject);
            currentIngredients.Add(other.gameObject);

            other.transform.SetParent(this.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (trackedObjects.Contains(other.gameObject))
        {
            trackedObjects.Remove(other.gameObject);
            currentIngredients.Remove(other.gameObject);

            Ingredient ingredient = other.GetComponent<Ingredient>();
            if (ingredient != null)
            {
                IngredientType type = ingredient.ingredientType;

                ingredientCounts[type]--;
                if (ingredientCounts[type] <= 0)
                {
                    ingredientCounts.Remove(type);
                    containedIngredientTypes.Remove(type);
                }
            }

            other.transform.SetParent(null);
        }
    }

    public bool HasContents()
    {
        return containedIngredientTypes.Count > 0;
    }

    public List<IngredientType> GetContainedIngredientTypes()
    {
        return new List<IngredientType>(containedIngredientTypes);
    }

    public void ClearContents()
    {
        foreach (GameObject obj in currentIngredients)
        {
            if (obj != null)
                Destroy(obj);
        }

        currentIngredients.Clear();
        containedIngredientTypes.Clear();
        trackedObjects.Clear();
        ingredientCounts.Clear();
    }

    public void SetContents(GameObject newCompoundPrefab)
    {
        ClearContents();

        if (newCompoundPrefab != null && contentsSpawnPoint != null)
        {
            GameObject compound = Instantiate(newCompoundPrefab, contentsSpawnPoint.position, Quaternion.identity);
            compound.transform.SetParent(this.transform);
        }
    }

    public void RegisterIngredient(IngredientType newIngredient)
    {
        if (!ingredientCounts.ContainsKey(newIngredient))
            ingredientCounts[newIngredient] = 0;

        ingredientCounts[newIngredient]++;

        if (!containedIngredientTypes.Contains(newIngredient))
        {
            containedIngredientTypes.Add(newIngredient);
            Debug.Log($"Ingredient added via pouring: {newIngredient}");
        }
    }
}
