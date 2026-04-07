using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private PlayerIdentity identity;
    private PlayerInput playerInput;
    private Vector2 moveInput;

    // Example key names for movement (set these in Inspector or code)
    public string moveLeftKey = "A";
    public string moveRightKey = "D";
    public string moveUpKey = "W";
    public string moveDownKey = "S";

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        identity = GetComponent<PlayerIdentity>();
    }

    void FixedUpdate()
    {
        Vector3 movement = Vector3.zero;

        // BEST PRACTICE: Don't check for playerIndex. 
        // The PlayerInput component already knows which device belongs to THIS player.
        if (playerInput != null)
        {
            // This will work for Controller 1, Controller 2, or Keyboard 
            // depending on what was used to "Join"
            moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
            movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * speed;
        }

        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    
    void OnCollisionEnter(Collision collision) {
        
        // return if colliding with other object or self
        PlayerIdentity otherIdentity = collision.gameObject.GetComponent<PlayerIdentity>();
        if (otherIdentity == null || otherIdentity == identity)
            return;

        //GetComponent<PlayerHaptics>()?.Pulse(0.5f, 0.2f);
    }

    private Key GetKey(string keyName)
    {
        if (System.Enum.TryParse(keyName, ignoreCase: true, out Key key))
            return key;

        Debug.LogWarning($"Invalid key name: '{keyName}', defaulting to None");
        return Key.None;
    }
}