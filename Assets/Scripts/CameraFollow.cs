using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    [Header("3rd Person Settings")]
    public Vector3 thirdPersonOffset = new Vector3(0, 5, -10);
    
    [Header("1st Person Settings")]
    public Vector3 firstPersonOffset = new Vector3(0, 1.8f, 0.3f);
    
    [Header("Camera Settings")]
    public float smoothSpeed = 5f;
    
    public enum CameraPOV { ThirdPerson, FirstPerson }
    public CameraPOV currentPOV = CameraPOV.ThirdPerson;
    
    private Vector3 currentOffset;
    private float rotationX = 0f;
    public float mouseSensitivity = 2f;

    void Start()
    {
        currentOffset = thirdPersonOffset;
    }
    
    void Update()
    {
        // Toggle POV with F5
        if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
        {
            TogglePOV();
        }
        
        // First person look control
        if (currentPOV == CameraPOV.FirstPerson && Mouse.current != null)
        {
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * 0.1f;
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -80f, 80f);
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        if (currentPOV == CameraPOV.ThirdPerson)
        {
            // 3rd person - follow behind player
            Vector3 desiredPosition = target.position + target.rotation * currentOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
        else
        {
            // 1st person - inside player head
            Vector3 desiredPosition = target.position + target.rotation * currentOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * 2f * Time.deltaTime);
            transform.rotation = Quaternion.Euler(rotationX, target.eulerAngles.y, 0);
        }
    }
    
    public void TogglePOV()
    {
        if (currentPOV == CameraPOV.ThirdPerson)
        {
            currentPOV = CameraPOV.FirstPerson;
            currentOffset = firstPersonOffset;
            Debug.Log("Camera: First Person");
        }
        else
        {
            currentPOV = CameraPOV.ThirdPerson;
            currentOffset = thirdPersonOffset;
            rotationX = 0f;
            Debug.Log("Camera: Third Person");
        }
    }
}