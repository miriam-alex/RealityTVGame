using UnityEngine;

// the different states the player will be in (resource chain is essentially a state machine thing right now)
public enum PlayerState
{
    StartingState,
    Tree,
    RawChair,
    FinishedChair
}

public class PlayerResourceChain : MonoBehaviour
{
    // default state: player is not in process of gathering materials/making chair
    public PlayerState currentState = PlayerState.StartingState;

    // players can only go to next state according to current state and the resources they currently have
    public void NextState(Resource resource)
    {
        if (resource.resourceName == "Wood" && currentState == PlayerState.StartingState)
        {
            currentState = PlayerState.Tree;
        }
        
        else if (resource.resourceName == "RawChair" && currentState == PlayerState.Tree)
        {
            currentState = PlayerState.RawChair;
        }
        
        else if (resource.resourceName == "FinishedChair" && currentState == PlayerState.RawChair)
        {
            currentState = PlayerState.FinishedChair;
        }
        
        Debug.Log(gameObject.name + " collected: " + resource.resourceName + " | current state: " + currentState);
    }
}