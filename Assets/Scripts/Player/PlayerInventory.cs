using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<Resource> resources = new List<Resource>();

    // 🔥 THIS is what your UI is trying to access
    public event Action OnInventoryChanged;

    public void AddResource(Resource resource)
    {
        if (resource == null) return;

        resources.Add(resource);

        Debug.Log(gameObject.name + " added: " + resource.resourceName);

        // Notify UI
        OnInventoryChanged?.Invoke();
    }

    public void RemoveResource(Resource resource)
    {
        if (resource == null) return;

        resources.Remove(resource);
        Debug.Log(gameObject.name + " removed: " + resource.resourceName);
        OnInventoryChanged?.Invoke();
    }


    public bool HasResource(Resource resource)
    {
        return resources.Contains(resource);
    }

    public bool HasResource(string resourceName)
    {
        foreach (var r in resources)
        {
            if (r.resourceName == resourceName)
                return true;
        }
        return false;
    }
}