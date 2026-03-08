using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Recipe
{
    [Header("Recipe Settings")]
    public string recipeName;
    
    [Header("Requirements")]
    public Resource[] requiredResources; // Resource ScriptableObjects needed
    public int[] requiredAmounts;        // How many of each
    
    [Header("Output")]
    public Resource outputResource;      // What Resource to create
    public int outputAmount = 1;
    
    [Header("Processing")]
    public float processingTime = 3f;    // How long it takes to process
}

public class ProcessingStation : MonoBehaviour
{
    [Header("Station Settings")]
    public string stationName = "Processing Station";
    public Recipe[] recipes;
    
    [Header("Containers")]
    public Container inputContainer;
    public Container outputContainer;
    
    [Header("Visual Feedback")]
    public Color idleColor = Color.white;
    public Color processingColor = Color.yellow;
    public Color readyColor = Color.green;
    
    private bool isProcessing = false;
    private float processingTimer = 0f;
    private Recipe currentRecipe;
    private Renderer stationRenderer;
    
    private void Start()
    {
        stationRenderer = GetComponent<Renderer>();
        if (stationRenderer == null)
        {
            stationRenderer = GetComponentInChildren<Renderer>();
        }
        
        Debug.Log($"[{stationName}] Station started with {recipes.Length} recipes");
        
        // Validate setup
        if (inputContainer == null)
            Debug.LogError($"[{stationName}] Input container is null!");
        if (outputContainer == null)
            Debug.LogError($"[{stationName}] Output container is null!");
            
        // Validate recipes
        for (int i = 0; i < recipes.Length; i++)
        {
            var recipe = recipes[i];
            if (recipe.requiredResources == null || recipe.requiredResources.Length == 0)
                Debug.LogError($"[{stationName}] Recipe {i} ({recipe.recipeName}) has no required resources!");
            if (recipe.requiredAmounts == null || recipe.requiredAmounts.Length != recipe.requiredResources.Length)
                Debug.LogError($"[{stationName}] Recipe {i} ({recipe.recipeName}) has mismatched required amounts!");
            if (recipe.outputResource == null)
                Debug.LogError($"[{stationName}] Recipe {i} ({recipe.recipeName}) has no output resource!");
            else if (recipe.outputResource.prefab == null)
                Debug.LogError($"[{stationName}] Recipe {i} ({recipe.recipeName}) output resource has no prefab!");
        }
            
        UpdateVisualFeedback();
    }
    
    private void Update()
    {
        if (isProcessing)
        {
            ProcessCurrentRecipe();
        }
        else
        {
            CheckForAvailableRecipes();
            
            // Only debug occasionally when we have items
            if (inputContainer != null && inputContainer.items.Count > 0 && Time.frameCount % 60 == 0)
            {
                var itemNames = new List<string>();
                foreach (var item in inputContainer.items)
                {
                    var resourceItem = item.GetComponent<ResourceItem>();
                    string resourceName = resourceItem?.resource?.resourceName ?? "NULL";
                    itemNames.Add($"{item.name}({resourceName})");
                }
                Debug.Log($"[{stationName}] Items in input: {string.Join(", ", itemNames)}");
            }
        }
    }
    
    private void ProcessCurrentRecipe()
    {
        processingTimer += Time.deltaTime;
        
        float progress = processingTimer / currentRecipe.processingTime;
        
        if (Time.frameCount % 60 == 0) // Debug every second
        {
            Debug.Log($"[{stationName}] Processing {currentRecipe.recipeName}: {progress:P1} ({processingTimer:F1}s / {currentRecipe.processingTime:F1}s)");
        }
        
        if (processingTimer >= currentRecipe.processingTime)
        {
            Debug.Log($"[{stationName}] Processing completed! Calling CompleteProcessing()");
            CompleteProcessing();
        }
        
        UpdateVisualFeedback();
    }
    
    private void CheckForAvailableRecipes()
    {
        if (inputContainer == null)
        {
            Debug.LogError($"[{stationName}] Input container is null in CheckForAvailableRecipes!");
            return;
        }
        
        if (inputContainer.items.Count == 0)
        {
            return; // No spam when empty
        }
        
        Debug.Log($"[{stationName}] Found {inputContainer.items.Count} items in input container");
        
        for (int i = 0; i < recipes.Length; i++)
        {
            var recipe = recipes[i];
            Debug.Log($"[{stationName}] Checking recipe {i}: {recipe.recipeName}");
            
            if (CanProcessRecipe(recipe))
            {
                Debug.Log($"[{stationName}] Recipe {recipe.recipeName} can be processed!");
                StartProcessing(recipe);
                return;
            }
            else
            {
                Debug.Log($"[{stationName}] Recipe {recipe.recipeName} cannot be processed");
            }
        }
        
        Debug.Log($"[{stationName}] No valid recipes found for current items");
    }
    
    private bool CanProcessRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogError($"[{stationName}] Recipe is null!");
            return false;
        }
        
        Debug.Log($"[{stationName}] Checking recipe: {recipe.recipeName}");
        
        // Check if we have all required resources in input container
        for (int i = 0; i < recipe.requiredResources.Length; i++)
        {
            Resource resource = recipe.requiredResources[i];
            int needed = recipe.requiredAmounts[i];
            int found = CountResourcesOfType(resource);
            
            Debug.Log($"[{stationName}] Need {needed}x {(resource ? resource.resourceName : "NULL")}, found {found}");
            
            if (found < needed)
            {
                Debug.Log($"[{stationName}] Not enough {(resource ? resource.resourceName : "NULL")}: need {needed}, have {found}");
                return false;
            }
        }
        
        // Check if output container has space
        bool hasSpace = outputContainer.items.Count + recipe.outputAmount <= outputContainer.capacity;
        Debug.Log($"[{stationName}] Output space check: {outputContainer.items.Count} + {recipe.outputAmount} <= {outputContainer.capacity} = {hasSpace}");
        
        return hasSpace;
    }
    
    private int CountResourcesOfType(Resource targetResource)
    {
        int count = 0;
        Debug.Log($"[{stationName}] Counting resources of type: {(targetResource ? targetResource.resourceName : "NULL")}");
        
        foreach (var item in inputContainer.items)
        {
            Debug.Log($"[{stationName}] Checking item: {item.name}");
            
            var resourceItem = item.GetComponent<ResourceItem>();
            if (resourceItem == null)
            {
                Debug.Log($"[{stationName}] Item {item.name} has no ResourceItem component!");
                continue;
            }
            
            if (resourceItem.resource == null)
            {
                Debug.Log($"[{stationName}] ResourceItem {item.name} has null resource!");
                continue;
            }
            
            Debug.Log($"[{stationName}] Item {item.name} contains resource: {resourceItem.resource.resourceName}");
            
            if (resourceItem.resource == targetResource)
            {
                count++;
                Debug.Log($"[{stationName}] Found matching resource! Count now: {count}");
            }
        }
        
        Debug.Log($"[{stationName}] Final count for {(targetResource ? targetResource.resourceName : "NULL")}: {count}");
        return count;
    }
    
    private void StartProcessing(Recipe recipe)
    {
        Debug.Log($"[{stationName}] Starting processing for recipe: {recipe.recipeName}");
        
        currentRecipe = recipe;
        isProcessing = true;
        processingTimer = 0f;
        
        // Remove required items from input container
        Debug.Log($"[{stationName}] Consuming input items...");
        ConsumeInputItems(recipe);
        
        Debug.Log($"[{stationName}] Processing started for {recipe.recipeName}, will take {recipe.processingTime} seconds");
    }
    
    private void ConsumeInputItems(Recipe recipe)
    {
        Debug.Log($"[{stationName}] ConsumeInputItems called for {recipe.recipeName}");
        
        for (int i = 0; i < recipe.requiredResources.Length; i++)
        {
            Resource resource = recipe.requiredResources[i];
            int amountNeeded = recipe.requiredAmounts[i];
            int consumed = 0;
            
            Debug.Log($"[{stationName}] Need to consume {amountNeeded}x {resource.resourceName}");
            
            for (int j = inputContainer.items.Count - 1; j >= 0 && consumed < amountNeeded; j--)
            {
                var resourceItem = inputContainer.items[j].GetComponent<ResourceItem>();
                if (resourceItem != null && resourceItem.resource == resource)
                {
                    Debug.Log($"[{stationName}] Destroying {resourceItem.resource.resourceName} (item {j})");
                    Destroy(inputContainer.items[j].gameObject);
                    inputContainer.items.RemoveAt(j);
                    consumed++;
                    Debug.Log($"[{stationName}] Consumed {consumed}/{amountNeeded}");
                }
            }
            
            if (consumed < amountNeeded)
            {
                Debug.LogError($"[{stationName}] Failed to consume enough {resource.resourceName}! Got {consumed}/{amountNeeded}");
            }
        }
        
        Debug.Log($"[{stationName}] Finished consuming items. Input container now has {inputContainer.items.Count} items");
    }
    
    private void CompleteProcessing()
    {
        Debug.Log($"[{stationName}] CompleteProcessing called for {currentRecipe.recipeName}");
        
        // Create output items using the Resource's prefab
        for (int i = 0; i < currentRecipe.outputAmount; i++)
        {
            Debug.Log($"[{stationName}] Creating output item {i + 1}/{currentRecipe.outputAmount}");
            
            if (currentRecipe.outputResource == null)
            {
                Debug.LogError($"[{stationName}] Output resource is null!");
                continue;
            }
            
            if (currentRecipe.outputResource.prefab == null)
            {
                Debug.LogError($"[{stationName}] Output resource {currentRecipe.outputResource.resourceName} has no prefab!");
                continue;
            }
            
            Debug.Log($"[{stationName}] Instantiating {currentRecipe.outputResource.resourceName} prefab");
            var outputObj = Instantiate(currentRecipe.outputResource.prefab);
            outputObj.transform.position = outputContainer.transform.position + Vector3.up * i * 0.5f;
            outputObj.transform.localScale = Vector3.one;
            
            var resourceItem = outputObj.GetComponent<ResourceItem>();
            if (resourceItem == null)
            {
                Debug.LogError($"[{stationName}] Created object has no ResourceItem component!");
                continue;
            }
            
            Debug.Log($"[{stationName}] Adding item to output container");
            bool added = outputContainer.AddItem(resourceItem);
            Debug.Log($"[{stationName}] Item added to container: {added}");
        }
        
        Debug.Log($"[{stationName}] Completed processing {currentRecipe.recipeName}! Output container now has {outputContainer.items.Count} items");
        
        isProcessing = false;
        currentRecipe = null;
        processingTimer = 0f;
        UpdateVisualFeedback();
    }
    
    private void UpdateVisualFeedback()
    {
        if (stationRenderer == null) return;
        
        if (isProcessing)
        {
            float progress = processingTimer / currentRecipe.processingTime;
            Color targetColor = Color.Lerp(processingColor, readyColor, progress);
            stationRenderer.material.color = targetColor;
        }
        else
        {
            stationRenderer.material.color = idleColor;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShowRecipeInfo();
        }
        
        // Add manual test key
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"[{stationName}] Manual test - Input container has {inputContainer.items.Count} items");
            CheckForAvailableRecipes();
        }
    }
    
    private void ShowRecipeInfo()
    {
        if (recipes.Length == 0)
        {
            Debug.Log($"{stationName}: No recipes available");
            return;
        }
        
        Debug.Log($"=== {stationName} Recipes ===");
        foreach (var recipe in recipes)
        {
            string requirements = "";
            for (int i = 0; i < recipe.requiredResources.Length; i++)
            {
                if (i > 0) requirements += ", ";
                requirements += $"{recipe.requiredAmounts[i]}x {recipe.requiredResources[i].resourceName}";
            }
            
            Debug.Log($"• {recipe.recipeName}: {requirements} → {recipe.outputAmount}x {recipe.outputResource.resourceName}");
        }
    }
}