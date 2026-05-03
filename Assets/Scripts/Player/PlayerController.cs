using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float rotationSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public float coyoteTime = 0.08f;

    private Rigidbody rb;
    private PlayerIdentity identity;
    private Animator animator;
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private bool isGrounded;
    private float lastGroundedTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        // Manually subscribe to the jump event
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions["Jump"].performed += OnJump;
        }
    }

    void OnDisable()
    {
        // Manually unsubscribe to prevent memory leaks
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions["Jump"].performed -= OnJump;
        }
    }

    private PlayerStatus status; // 1. Add this line

    void Start()
    {
        identity = GetComponent<PlayerIdentity>();
        status = GetComponent<PlayerStatus>(); // 2. Add this line
        animator = identity.animator;

        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck transform is not assigned on PlayerController. Please create an empty GameObject as a child of the player, position it at the player's feet, and assign it to the 'groundCheck' field.");
            enabled = false; // Disable the script to prevent errors
            return;
        }
    }

    void Update()
    {
        isGrounded = Time.time <= lastGroundedTime + coyoteTime;
    }

    void FixedUpdate()
    {

        Vector3 movement = Vector3.zero;
        if (playerInput != null)
        {
            moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
            movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * speed;
        }
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        if (movement != Vector3.zero)
        {
            animator.SetBool("isRunning", true);
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed || !isGrounded)
        {
            Debug.Log($"Jump Failed! ContextPerformed: {context.performed}, isGrounded: {isGrounded}");
            return;
        }

        Debug.Log("Jump Executed!");
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        isGrounded = false;
        animator.SetBool("isJumping", true);
        lastGroundedTime = -999f;
    }

    void OnCollisionStay(Collision collision)
    {
        TryMarkGrounded(collision);
    }

    private void TryMarkGrounded(Collision collision)
    {
        bool isGroundLayer = (groundLayer.value & (1 << collision.gameObject.layer)) != 0;
        if (!isGroundLayer) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            if (contact.normal.y >= 0.35f)
            {
                lastGroundedTime = Time.time;
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        TryMarkGrounded(collision);
        animator.SetBool("isJumping", false);
        
        // return if colliding with other object or self
        PlayerIdentity otherIdentity = collision.gameObject.GetComponent<PlayerIdentity>();
        if (otherIdentity == null || otherIdentity == identity)
            return;

        // GetComponent<PlayerHaptics>()?.Pulse(0.5f, 0.2f);
    }

    private Key GetKey(string keyName)
    {
        if (System.Enum.TryParse(keyName, ignoreCase: true, out Key key))
            return key;

        Debug.LogWarning($"Invalid key name: '{keyName}', defaulting to None");
        return Key.None;
    }
}