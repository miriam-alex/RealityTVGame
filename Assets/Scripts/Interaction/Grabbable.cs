using UnityEngine;
using UnityEngine.Rendering.Universal; 

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour, IInteractable
{
    public RenderingLayerMask outlineLayer; 
    private Rigidbody _rb;
    private MeshRenderer _meshRenderer;
    private bool _isGrabbed = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    
    public bool IsAvailable(PlayerIdentity requester) => !_isGrabbed;
    public float GetHoldDuration(PlayerIdentity requester) => 0f;
    
    public InteractionPromptData GetInteractionPromptData(PlayerIdentity requester)
        => InteractionPromptData.PrimaryOnly("Pick Up");

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

    public void ShowOutline()
    {
        Debug.Log("showing outline");
        _meshRenderer.renderingLayerMask |= (uint)outlineLayer.value;
    }

    public void HideOutline()
    {
        Debug.Log("hiding outline");
        _meshRenderer.renderingLayerMask &= ~(uint)outlineLayer.value;
    }

}