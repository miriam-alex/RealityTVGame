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
    private bool useController = false;

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
        // Check for controller connection (only for Player 1)
        if (identity != null && identity.playerIndex == 0)
        {
            useController = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
            Debug.Log("using controller: " + useController);
        }

        float moveX = 0f;
        float moveZ = 0f;

        if (useController)
        {
            // Use left stick for movement
            Vector2 stick = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
            moveX = stick.x;
            moveZ = stick.y;
        }
        else
        {
            if (Keyboard.current[GetKey(moveLeftKey)].isPressed)
                moveX = -1f;
            else if (Keyboard.current[GetKey(moveRightKey)].isPressed)
                moveX = 1f;

            if (Keyboard.current[GetKey(moveUpKey)].isPressed)
                moveZ = 1f;
            else if (Keyboard.current[GetKey(moveDownKey)].isPressed)
                moveZ = -1f;
        }

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized * speed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    
    void OnCollisionEnter(Collision collision) {
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