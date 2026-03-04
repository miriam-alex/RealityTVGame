using System;
using UnityEngine;

public class TreeStation : MonoBehaviour
{

    public Resource wood;

    private void OnTriggerStay(Collider other)
    {
        // placeholder mechanic (player clicks 'E' key when at Tree station to get wood for RawChair station
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            var chain = other.GetComponent <PlayerResourceChain>();
            if (chain != null && chain.currentState == PlayerState.StartingState)
            {
                // advance to next state with wood resource
                Debug.Log(gameObject.name + " collected " + wood);
                chain.NextState(wood);
            }
        }
        
    }
}
