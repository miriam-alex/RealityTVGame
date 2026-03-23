using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour, IInteractable
{
    private Rigidbody _rb;
    private PlayerIdentity _myId;
    private bool _isGrabbed = false;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public bool IsAvailable(PlayerIdentity requester) => !_isGrabbed;
    public float GetHoldDuration(PlayerIdentity requester) => 0f;
    
    public string GetInteractionPrompt(string i, string a) => $"[{i}] Pick Up";

    public void Interact(PlayerIdentity requester)
    {
        if (!IsAvailable(requester)) return;

        if (requester.TryGetComponent(out PlayerInventory inventory))
        {
            inventory.AddItem(this);
        }
        
    }

    public void AltInteract(PlayerIdentity requester) { /* Optional */ }

    // This is called BY the inventory
    public void OnPickedUp()
    {
        _isGrabbed = true;
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.detectCollisions = false; // Prevents the player from tripping over it
    }

    // This is called BY the inventory
    public void OnDropped()
    {
        _isGrabbed = false;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.detectCollisions = true;
    }
    
}