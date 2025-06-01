using System.Collections.Generic;
using UnityEngine;

// Manages the contents of a container like a beaker or test tube.
// Tracks ingredients (both solid GameObjects and poured liquid types),
// handles automatic parenting, and spawns resulting compounds.
public class Container : MonoBehaviour
{
    [Header("Ingredients")]
    public Transform contentsSpawnPoint;

    // List of GameObjects (physical solid ingredients) inside the container
    public List<GameObject> currentIngredients = new List<GameObject>();

    // Unique list of ingredient types currently in the container (solids and liquids)
    public List<IngredientType> containedIngredientTypes = new List<IngredientType>();

    // Tracks which GameObjects have been registered to avoid duplicates
    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();

    // Called when an ingredient object enters the container trigger.
    // Tracks solid ingredients and attaches them as children.
    private void OnTriggerEnter(Collider other)
    {
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient != null && !trackedObjects.Contains(other.gameObject))
        {
            IngredientType type = ingredient.ingredientType;

            // Register new type if not already present
            if (!containedIngredientTypes.Contains(type))
                containedIngredientTypes.Add(type);

            trackedObjects.Add(other.gameObject);
            currentIngredients.Add(other.gameObject);

            // Parent to container visually
            other.transform.SetParent(this.transform);
        }
    }

    // Called when an ingredient object exits the container trigger.
    // Unregisters the GameObject and updates ingredient type tracking.
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

                // Only remove type if no other objects of that type are left
                bool stillPresent = false;
                foreach (GameObject obj in currentIngredients)
                {
                    Ingredient i = obj.GetComponent<Ingredient>();
                    if (i != null && i.ingredientType == type)
                    {
                        stillPresent = true;
                        break;
                    }
                }

                if (!stillPresent)
                    containedIngredientTypes.Remove(type);
            }

            // Unparent for world space behavior
            other.transform.SetParent(null);
        }
    }

    // Returns true if the container currently holds any ingredients.
    public bool HasContents()
    {
        return containedIngredientTypes.Count > 0;
    }

    // Returns a copy of the list of ingredient types currently in the container.
    public List<IngredientType> GetContainedIngredientTypes()
    {
        return new List<IngredientType>(containedIngredientTypes);
    }

    // Used to manually set the contents of the container to a resulting compound prefab.
    //Destroys previous contents and spawns a new compound at the spawn point.
    public void SetContents(GameObject newCompoundPrefab)
    {
        // Destroy all current physical ingredients
        foreach (GameObject obj in currentIngredients)
        {
            if (obj != null)
                Destroy(obj);
        }

        currentIngredients.Clear();
        containedIngredientTypes.Clear();
        trackedObjects.Clear();

        // Spawn the resulting compound
        if (newCompoundPrefab != null && contentsSpawnPoint != null)
        {
            GameObject compound = Instantiate(newCompoundPrefab, contentsSpawnPoint.position, Quaternion.identity);
            compound.transform.SetParent(this.transform);
        }
    }

    // Allows liquid-pouring bottles to register an ingredient type via trigger.
    // Called by LiquidIngredientTag.cs.
    public void RegisterIngredient(IngredientType newIngredient)
    {
        if (!containedIngredientTypes.Contains(newIngredient))
        {
            containedIngredientTypes.Add(newIngredient);
            Debug.Log($"Ingredient added via pouring: {newIngredient}");
        }
    }
}
