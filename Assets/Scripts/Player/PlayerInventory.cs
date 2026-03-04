using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<Resource> resources = new List<Resource>();

    public void AddResource(Resource resource)
    {
        resources.Add(resource);
        Debug.Log(gameObject.name + " added: " + resource.resourceName);
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
            {
                return true;
            }
        }
        return false;
    }
}
