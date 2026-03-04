using System;
using UnityEngine;

public class RawChairStation : MonoBehaviour
{

    public Resource rawChair;

    private void OnTriggerStay(Collider other)
    {
        // placeholder mechanic (player clicks 'E' key when at RawChair station to get rawChair for FinalChair station)
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            var chain = other.GetComponent <PlayerResourceChain>();
            if (chain != null && chain.currentState == PlayerState.Tree)
            {
                // advance to next state with rawChair resource
                Debug.Log(gameObject.name + " collected " + rawChair);
                chain.NextState(rawChair);
            }
        }
        
    }
}