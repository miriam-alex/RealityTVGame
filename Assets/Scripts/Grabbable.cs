using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    private Rigidbody _rb;
    private Collider[] _colliders; // Store all colliders on the chair
    private Transform _objectGrabTransform;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Get all colliders (including the mesh and your new box trigger)
        _colliders = GetComponentsInChildren<Collider>();
    }

    public void Grab(Transform objectGrabTransform)
    {
        _objectGrabTransform = objectGrabTransform;
        _rb.isKinematic = true;
        _rb.useGravity = false;

        // Turn off colliders so it doesn't hit the player's body
        foreach (var col in _colliders)
        {
            col.enabled = false;
        }
    }

    public void Drop()
    {
        _objectGrabTransform = null;
        _rb.isKinematic = false;
        _rb.useGravity = true;

        // Turn colliders back on so it can land on the floor
        foreach (var col in _colliders)
        {
            col.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        if (_objectGrabTransform != null)
        {
            // Note: Use fixedDeltaTime inside FixedUpdate for smoother movement
            float lerpSpeed = 20f; 
            Vector3 newPosition = Vector3.Lerp(transform.position, _objectGrabTransform.position, Time.fixedDeltaTime * lerpSpeed);
            _rb.MovePosition(newPosition);
        }
    }
}