using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public List<ResourceItem> items = new List<ResourceItem>();
    public int capacity = 10;
    public bool autoDetectItems = true; // Automatically detect ResourceItems that enter trigger

    private void Start()
    {
        // Ensure we have a trigger collider for auto-detection
        if (autoDetectItems)
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider>();
                Debug.Log($"[Container {name}] Added BoxCollider for auto-detection");
            }
            col.isTrigger = true;
            Debug.Log($"[Container {name}] Container set up for auto-detection, capacity: {capacity}");
        }
    }

    public bool AddItem(ResourceItem item)
    {
        if (items.Count >= capacity)
        {
            Debug.Log($"[Container {name}] Cannot add {item.name} - container full ({items.Count}/{capacity})");
            return false;
        }
        
        items.Add(item);
        Debug.Log($"[Container {name}] Added {item.name} - container now has {items.Count}/{capacity} items");
        
        // List all items in container
        Debug.Log($"[Container {name}] Current items: {string.Join(", ", items.ConvertAll(i => i.name))}");
        
        return true;
    }

    public ResourceItem RemoveItem()
    {
        if (items.Count == 0)
        {
            Debug.Log($"[Container {name}] Cannot remove item - container empty");
            return null;
        }
        
        var item = items[0];
        items.RemoveAt(0);
        Debug.Log($"[Container {name}] Removed {item.name} - container now has {items.Count}/{capacity} items");
        return item;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!autoDetectItems) return;
        
        Debug.Log($"[Container {name}] Trigger entered by: {other.name}");
        
        ResourceItem resourceItem = other.GetComponent<ResourceItem>();
        if (resourceItem != null)
        {
            Debug.Log($"[Container {name}] Found ResourceItem: {resourceItem.name} with resource: {(resourceItem.resource ? resourceItem.resource.resourceName : "NULL")}");
            
            // Check if item is not already in container
            if (!items.Contains(resourceItem))
            {
                bool added = AddItem(resourceItem);
                if (added)
                {
                    // Position item inside container visually
                    other.transform.position = transform.position + Vector3.up * (items.Count * 0.3f);
                    
                    // Disable rigidbody so it doesn't fall out
                    Rigidbody rb = other.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                }
            }
            else
            {
                Debug.Log($"[Container {name}] Item {resourceItem.name} already in container");
            }
        }
        else
        {
            Debug.Log($"[Container {name}] Object {other.name} has no ResourceItem component");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!autoDetectItems) return;
        
        ResourceItem resourceItem = other.GetComponent<ResourceItem>();
        if (resourceItem != null && items.Contains(resourceItem))
        {
            items.Remove(resourceItem);
            Debug.Log($"[Container {name}] Removed {resourceItem.name} due to trigger exit - container now has {items.Count}/{capacity} items");
            
            // Re-enable rigidbody
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }
}
