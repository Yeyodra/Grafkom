using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float mouseSensitivity = 0.1f;
    
    private float rotationY = 0f;
    
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;
        
        float moveX = 0f, moveZ = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX = 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ = -1f;
        
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        transform.position += move * moveSpeed * Time.deltaTime;
        
        float mouseX = mouse.delta.x.ReadValue() * mouseSensitivity;
        rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        
        if (keyboard.escapeKey.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.None;
    }
}
