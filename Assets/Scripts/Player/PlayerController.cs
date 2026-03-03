using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float rotationSpeed = 10f;

    [Header("Key Bindings")]
    public string moveLeftKey = "A";
    public string moveRightKey = "D";
    public string moveUpKey = "W";
    public string moveDownKey = "S";

    private Rigidbody rb;
    private PlayerIdentity identity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Start()
    {
        identity = GetComponent<PlayerIdentity>();
    }

    void FixedUpdate()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current[GetKey(moveLeftKey)].isPressed)
            moveX = -1f;
        else if (Keyboard.current[GetKey(moveRightKey)].isPressed)
            moveX = 1f;

        if (Keyboard.current[GetKey(moveUpKey)].isPressed)
            moveZ = 1f;
        else if (Keyboard.current[GetKey(moveDownKey)].isPressed)
            moveZ = -1f;

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