using UnityEngine;
using System.Collections.Generic;

public class InteractionCoordinator : MonoBehaviour
{
    public static InteractionCoordinator Instance { get; private set; }
    
    // Players currently holding the button and looking at each other
    private HashSet<PlayerIdentity> _handshake = new HashSet<PlayerIdentity>();
    private HashSet<PlayerIdentity> _activeTraders = new HashSet<PlayerIdentity>();

    private void Awake() => Instance = this;

    public void Reset()
    {
        _handshake.Clear();
        _activeTraders.Clear();
    }

    private void OnDestroy()
    {
        Reset();
    }

    public void SetHandshake(PlayerIdentity p1, PlayerIdentity p2, bool active)
    {
        if (active) 
        {
            _handshake.Add(p1);
            _handshake.Add(p2);
        }
        else 
        {
            _handshake.Remove(p1);
            _handshake.Remove(p2);
        }
    }

    public bool IsTradeReady(PlayerIdentity p1, PlayerIdentity p2)
    {
        // Ready if both are in the handshake AND no one is busy
        return _handshake.Contains(p1) && _handshake.Contains(p2) &&
               !_activeTraders.Contains(p1) && !_activeTraders.Contains(p2);
    }

    public bool TryTrade(PlayerIdentity p1, PlayerIdentity p2, System.Action onTrade)
    {
        if (!IsTradeReady(p1, p2)) return false;

        _activeTraders.Add(p1);
        _activeTraders.Add(p2);

        // Hard-coded: This makes the Coordinator rely on PlayerInventory
        bool tradeSucceeded = p1.GetComponent<PlayerInventory>().TransferToPlayerInventory(p2.GetComponent<PlayerInventory>(), out _);

        _activeTraders.Remove(p1);
        _activeTraders.Remove(p2);
        return tradeSucceeded;
    }
    
    public bool CanInteract(PlayerIdentity p1, PlayerIdentity p2)
    {
        // A player is available if they are NOT in the active trade set
        bool canInteract = !_activeTraders.Contains(p1) && !_activeTraders.Contains(p2);
        return canInteract;
    }
}