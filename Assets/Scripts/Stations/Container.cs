using System;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public List<ResourceItem> items = new List<ResourceItem>();
    public Action<ResourceItem> OnItemEntered; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ResourceItem rItem))
        {
            // If someone is listening (like the Processing Station), tell them!
            if (OnItemEntered != null)
            {
                OnItemEntered.Invoke(rItem);
            }
            else
            {
                // Otherwise, handle it the old way for Input/Output stations
                if (!items.Contains(rItem))
                {
                    items.Add(rItem);
                    // Only freeze it if it's NOT being handled by a special station
                    if (other.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ResourceItem rItem))
        {
            if (items.Contains(rItem))
            {
                items.Remove(rItem);
                if (other.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
            }
        }
    }
    
    // Call this to manually "un-stick" an item if a player grabs it
    public void RemoveItem(ResourceItem item)
    {
        if (items.Contains(item)) items.Remove(item);
    }
}