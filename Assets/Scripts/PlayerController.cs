using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float mouseSensitivity = 0.1f;
    
    [Header("Flying Settings")]
    public float flySpeed = 15f;
    public float flyVerticalSpeed = 10f;
    public float doubleTapTime = 0.3f;
    
    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = ~0; // All layers by default
    private bool isGrounded = false;
    
    [Header("Collider Reference")]
    private CapsuleCollider playerCollider;
 // Max time between taps to trigger fly
    
    private float rotationY = 0f;
    private bool isFlying = false;
    private float lastSpacePressTime = -1f;
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;
        playerCollider = GetComponent<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;
        
        // Double-tap Space to toggle fly mode
        HandleFlyToggle(keyboard);
        
        // Horizontal movement (WASD)
        float moveX = 0f, moveZ = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX = 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ = -1f;
        
        float currentSpeed = isFlying ? flySpeed : moveSpeed;
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // Vertical movement when flying
        float moveY = 0f;
        if (isFlying)
        {
            if (keyboard.spaceKey.isPressed) moveY = 1f;  // Space to go up
            if (keyboard.leftShiftKey.isPressed || keyboard.leftCtrlKey.isPressed) moveY = -1f; // Shift/Ctrl to go down
            
            move += Vector3.up * moveY;
            
            // Disable gravity while flying
            if (rb != null)
            {
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
            }
        }
        else
        {
            // Re-enable gravity when not flying
            if (rb != null)
            {
                rb.useGravity = true;
            }
        }
        
        // Use physics-based movement when grounded, direct transform when flying
        if (rb != null && !isFlying)
        {
            Vector3 velocity = move.normalized * currentSpeed;
            velocity.y = rb.linearVelocity.y; // preserve gravity
            rb.linearVelocity = velocity;
        }
        else
        {
            transform.position += move.normalized * currentSpeed * Time.deltaTime;
        }
        
        // Mouse look
        float mouseX = mouse.delta.x.ReadValue() * mouseSensitivity;
        rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        
        if (keyboard.escapeKey.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.None;
    }
    
void HandleFlyToggle(Keyboard keyboard)
    {
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            float timeSinceLastPress = Time.time - lastSpacePressTime;
            
            // Check if this is a double-tap
            if (timeSinceLastPress <= doubleTapTime && lastSpacePressTime > 0)
            {
                isFlying = !isFlying;
                Debug.Log(isFlying ? "FLY MODE ON! (Space=Up, Shift=Down)" : "FLY MODE OFF");
                lastSpacePressTime = -1f; // Reset to prevent triple-tap toggle
            }
            else
            {
                lastSpacePressTime = Time.time;
                
                // Single tap = Jump (jika tidak flying dan di ground)
                if (!isFlying && isGrounded && rb != null)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    isGrounded = false;
                }
            }
        }
    }

void FixedUpdate()
    {
        // Ground check menggunakan raycast dari bawah player
        if (rb != null && !isFlying)
        {
            // Calculate ground check distance based on actual collider height
            float checkDistance = groundCheckDistance;
            if (playerCollider != null)
            {
                checkDistance = (playerCollider.height / 2f) + groundCheckDistance;
            }
            isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Fallback ground detection via collision
        if (!isFlying)
        {
            isGrounded = true;
        }
    }

}
