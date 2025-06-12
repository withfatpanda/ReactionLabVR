using System.Collections.Generic;
using UnityEngine;

// This script is used on containers like beakers or flasks that can hold ingredients
// including liquids poured via rotation or solids dropped in
public class Container : MonoBehaviour
{
    [Header("Spawn Point for Compounds")]
    public Transform contentsSpawnPoint;

    [Header("Pouring Settings")]
    public Transform pourOriginPoint;      // Where the pour stream starts (e.g., beaker lip)
    public float pourAngleThreshold = 100f; // Angle from upright to begin pouring
    public float pourCooldown = 1.0f;       // Delay between pours

    [Header("Liquid Properties")]
    public IngredientType currentLiquidIngredient; // The liquid this container holds (if any)
    public bool hasLiquid = false;                 // If the container is holding liquid

    private float pourTimer = 0f;

    // Ingredient tracking
    public List<GameObject> currentIngredients = new List<GameObject>();     // Physical objects like Salt
    public List<IngredientType> containedIngredientTypes = new List<IngredientType>(); // All unique ingredients
    private Dictionary<IngredientType, int> ingredientCounts = new Dictionary<IngredientType, int>(); // Quantity tracking
    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();  // Prevents duplicate entries

    private void Update()
    {
        pourTimer -= Time.deltaTime;

        // Detect pouring angle — if tilted past threshold and holding liquid, try to pour
        float angle = Vector3.Angle(transform.up, Vector3.up);
        if (hasLiquid && angle > pourAngleThreshold && pourTimer <= 0f)
        {
            TryPourLiquid();
            pourTimer = pourCooldown;
        }

        // Optional: update shader fill level here
        // e.g. GetComponent<Renderer>().material.SetFloat("_FillAmount", calculatedFill);
    }

    // Called when the container is tilted enough to pour
    private void TryPourLiquid()
    {
        if (pourOriginPoint == null) return;

        RaycastHit hit;
        // Cast ray downward to detect a receiving container
        if (Physics.Raycast(pourOriginPoint.position, Vector3.down, out hit, 0.5f))
        {
            Container targetContainer = hit.collider.GetComponentInParent<Container>();
            if (targetContainer != null && targetContainer != this)
            {
                targetContainer.ReceiveLiquid(currentLiquidIngredient);
                Debug.Log($"Poured {currentLiquidIngredient} into {targetContainer.name}");
            }
        }
    }

    // Called when this container receives liquid from another
    public void ReceiveLiquid(IngredientType type)
    {
        if (!ingredientCounts.ContainsKey(type))
            ingredientCounts[type] = 0;

        ingredientCounts[type]++;
        if (!containedIngredientTypes.Contains(type))
            containedIngredientTypes.Add(type);
    }

    // Called when a solid object enters this container
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

            // Make the object a child of the container for stability
            other.transform.SetParent(this.transform);
        }
    }

    // Called when a solid object leaves the container
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

    // Returns true if the container has any ingredients inside
    public bool HasContents()
    {
        return containedIngredientTypes.Count > 0;
    }

    // Called by machines to get what's inside
    public List<IngredientType> GetContainedIngredientTypes()
    {
        return new List<IngredientType>(containedIngredientTypes);
    }

    // Clears out current contents
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

        hasLiquid = false;
    }

    // Used when a machine reaction finishes and places a result back in the container
    public void SetContents(GameObject newCompoundPrefab)
    {
        ClearContents();

        if (newCompoundPrefab != null && contentsSpawnPoint != null)
        {
            GameObject compound = Instantiate(newCompoundPrefab, contentsSpawnPoint.position, Quaternion.identity);
            compound.transform.SetParent(this.transform);
        }
    }

    // Called when this container is filled with a liquid (via UI, valve, etc.)
    public void SetLiquid(IngredientType type)
    {
        currentLiquidIngredient = type;
        hasLiquid = true;

        // Also logs it in ingredients for MachineReactor compatibility
        ReceiveLiquid(type);
    }
}
