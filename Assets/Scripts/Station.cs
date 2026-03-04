using UnityEngine;

public class Station : MonoBehaviour
{
    [Header("Station Settings")]
    // resource produced by this station
    public Resource resourceProduced;

    public Resource resourceRequired;
    
    // player must be in the correct state to make specific resource
    public PlayerState requiredPreviousState;

    private void OnTriggerStay(Collider other)
    {
        // non-players cannot interact with the stations
        if (!other.CompareTag("Player")) return;
        
        var chain = other.GetComponent <PlayerResourceChain>();
        var inventory = other.GetComponent<PlayerInventory>();

        // there has to be 
        if (chain == null || inventory == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (resourceRequired != null && !inventory.HasResource(resourceRequired))
            {
                Debug.Log("Resource required: " + resourceRequired.resourceName);
                return;
            }

            if (chain.currentState != requiredPreviousState)
            {
                Debug.Log("Wrong state! Required: " + requiredPreviousState);
                return;
            }
            
            // add resource to inventory 
            inventory.RemoveResource(resourceRequired);
            inventory.AddResource(resourceProduced);
            chain.NextState(resourceProduced);
        }
    }
}
