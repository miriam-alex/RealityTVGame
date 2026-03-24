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

        // adds haptics component at start of game
        if (GetComponent<PlayerHaptics>() == null)
            gameObject.AddComponent<PlayerHaptics>();
    }

    void FixedUpdate()
    {
        Vector3 movement = Vector3.zero;

        // NEED TO FIX: for testing purposes, we are checking per playerIndex
        // controllers need to work for both players (it does, I had to change the playerIndex to 1 to check for the other player)
        // when checking

        if (identity != null && identity.playerIndex == 0)
        {
            if (playerInput != null)
                moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
            else
                moveInput = Vector2.zero;
            movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * speed;
        }
        else
        {
            // for testing reasons, keeping option to move using keyboard for PLAYER 2 (IJKL)
            float moveX = 0f, moveZ = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current[GetKey(moveLeftKey)].isPressed) moveX = -1f;
                else if (Keyboard.current[GetKey(moveRightKey)].isPressed) moveX = 1f;
                if (Keyboard.current[GetKey(moveUpKey)].isPressed) moveZ = 1f;
                else if (Keyboard.current[GetKey(moveDownKey)].isPressed) moveZ = -1f;
            }
            movement = new Vector3(moveX, 0f, moveZ).normalized * speed;
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

        GetComponent<PlayerHaptics>()?.Pulse(0.5f, 0.2f);
        
        if (identity != null) 
            Debug.Log($"P{identity.playerIndex + 1}: Collided with {collision.collider.name}");
    }

    private Key GetKey(string keyName)
    {
        if (System.Enum.TryParse(keyName, ignoreCase: true, out Key key))
            return key;

        Debug.LogWarning($"Invalid key name: '{keyName}', defaulting to None");
        return Key.None;
    }
}