using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct ResourceRequirement
{
    public Resource resource; 
    public int amount;        
}

[System.Serializable]
public class Recipe
{
    [Header("Identity")]
    public string recipeName;

    [Header("Input Requirements")]
    public ResourceRequirement[] ingredients;

    [Header("Output")]
    public Resource outputResource;
    public int outputAmount = 1;

    [Header("Settings")]
    public float processingTime = 3f;
}

public class ProcessingStation : MonoBehaviour
{
    public Recipe[] recipes;
    public Container inputContainer;
    public Transform spawnPoint; 
    
    private Dictionary<Resource, int> _digitalInventory = new Dictionary<Resource, int>();
    private bool _isProcessing = false;

    private void OnEnable()
    {
        if (inputContainer != null)
            inputContainer.OnItemEntered += HandleVacuum; 
    }

    private void OnDisable()
    {
        if (inputContainer != null)
            inputContainer.OnItemEntered -= HandleVacuum;
    }

    private void HandleVacuum(ResourceItem item)
    {
        if (item == null || item.resource == null) return;

        // FIX: Check if this resource is actually needed for ANY recipe
        bool isIngredient = false;
        foreach (var recipe in recipes)
        {
            foreach (var req in recipe.ingredients)
            {
                if (req.resource == item.resource)
                {
                    isIngredient = true;
                    break;
                }
            }
            if (isIngredient) break;
        }

        // If it's not an ingredient, ignore it (don't destroy it!)
        if (!isIngredient) return;

        // 1. Convert to data
        if (!_digitalInventory.ContainsKey(item.resource)) _digitalInventory[item.resource] = 0;
        _digitalInventory[item.resource]++;

        // 2. DESTROY immediately now that we know we need it
        Destroy(item.gameObject);

        if (!_isProcessing) CheckRecipes();
    }

    private void CheckRecipes()
    {
        foreach (var recipe in recipes)
        {
            if (CanCraft(recipe))
            {
                StartCoroutine(CraftRoutine(recipe));
                break;
            }
        }
    }

    private bool CanCraft(Recipe recipe)
    {
        foreach (var req in recipe.ingredients)
        {
            if (!_digitalInventory.ContainsKey(req.resource) || _digitalInventory[req.resource] < req.amount)
                return false;
        }
        return true;
    }

    private IEnumerator CraftRoutine(Recipe recipe)
    {
        _isProcessing = true;

        foreach (var req in recipe.ingredients)
            _digitalInventory[req.resource] -= req.amount;

        yield return new WaitForSeconds(recipe.processingTime);

        // 3. SPAWN
        if (recipe.outputResource != null && recipe.outputResource.prefab != null)
        {
            // Spawn offset: ensure it's high enough to not hit the input trigger immediately
            Vector3 finalPos = spawnPoint.position + (Vector3.up * 0.75f);
            GameObject result = Instantiate(recipe.outputResource.prefab, finalPos, Quaternion.identity);
            
            if (result.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero;
                
                // Add a small horizontal push to move it away from the "mouth"
                rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
            }
        }

        _isProcessing = false;
        CheckRecipes();
    }
}