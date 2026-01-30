using UnityEngine;
using UnityEngine.InputSystem;

public class GarbageTruck : MonoBehaviour
{
    [Header("Settings")]
    public string questId = "collect_trash";
    public int totalTrashDelivered = 0;
    public int targetTrash = 5;
    
    [Header("UI")]
    public GameObject dropPrompt;
    
    [Header("Drive Away")]
    public float driveSpeed = 5f;
    public Vector3 driveDirection = Vector3.right;
    
    private bool playerInZone = false;
    private bool isDriving = false;
    private Transform dropZone;
    
    void Start()
    {
        // Find or create drop zone
        dropZone = transform.Find("DropZone");
        if (dropZone == null)
        {
            GameObject dz = new GameObject("DropZone");
            dz.transform.SetParent(transform);
            dz.transform.localPosition = new Vector3(1.2f, 1.5f, 0);
            
            BoxCollider col = dz.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(8f, 4f, 8f); // Lebih besar agar mudah detect player
            
            dropZone = dz.transform;
        }
        
        // Add trigger handler ke DropZone
        DropZoneTrigger trigger = dropZone.gameObject.GetComponent<DropZoneTrigger>();
        if (trigger == null)
        {
            trigger = dropZone.gameObject.AddComponent<DropZoneTrigger>();
        }
        trigger.truck = this;
        
        // Create drop prompt jika belum ada
        CreateDropPrompt();
        HidePrompt();
    }
    
    void CreateDropPrompt()
    {
        if (dropPrompt != null) return;
        
        // Gunakan DialogueManager prompt jika ada
        if (DialogueManager.Instance != null)
        {
            // Reuse existing prompt system
            return;
        }
        
        // Buat simple world-space prompt
        dropPrompt = new GameObject("DropPrompt");
        dropPrompt.transform.SetParent(transform);
        dropPrompt.transform.localPosition = new Vector3(0, 3.5f, 0);
        
        // Add TextMesh untuk world-space text
        TextMesh textMesh = dropPrompt.AddComponent<TextMesh>();
        textMesh.text = "Tekan E untuk buang sampah";
        textMesh.fontSize = 32;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        
        // Billboard behavior
        dropPrompt.AddComponent<BillboardText>();
    }
    
    void Update()
    {
        if (isDriving)
        {
            // Drive away animation
            transform.Translate(driveDirection * driveSpeed * Time.deltaTime, Space.World);
            return;
        }
        
        if (playerInZone && Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryDropTrash();
            }
        }
    }
    
    public void OnPlayerEnter()
    {
        playerInZone = true;
        
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItems())
        {
            ShowPrompt();
        }
    }
    
    public void OnPlayerExit()
    {
        playerInZone = false;
        HidePrompt();
    }
    
    void TryDropTrash()
    {
        if (InventoryManager.Instance == null) return;
        if (!InventoryManager.Instance.HasItems())
        {
            Debug.Log("Tidak ada sampah untuk dibuang!");
            return;
        }
        
        // Get count sebelum clear
        int count = InventoryManager.Instance.GetCount();
        
        // Clear inventory
        InventoryManager.Instance.ClearItems();
        
        // Update total delivered
        totalTrashDelivered += count;
        
        Debug.Log($"Sampah dibuang! Total: {totalTrashDelivered}/{targetTrash}");
        
        // Update quest progress
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnTrashDelivered(count);
        }
        
        // Hide prompt
        HidePrompt();
        
        // Check if quest complete
        if (totalTrashDelivered >= targetTrash)
        {
            Debug.Log("Semua sampah sudah dibuang!");
            // Quest completion akan di-handle oleh QuestManager
        }
    }
    
    void ShowPrompt()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowPrompt("Tekan E untuk buang sampah");
        }
        else if (dropPrompt != null)
        {
            dropPrompt.SetActive(true);
        }
    }
    
    void HidePrompt()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.HidePrompt();
        }
        else if (dropPrompt != null)
        {
            dropPrompt.SetActive(false);
        }
    }
    
    public void DriveAway()
    {
        isDriving = true;
        HidePrompt();
        
        // Destroy setelah beberapa detik
        Destroy(gameObject, 10f);
    }
}

// Separate class untuk trigger handling
public class DropZoneTrigger : MonoBehaviour
{
    public GarbageTruck truck;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && truck != null)
        {
            truck.OnPlayerEnter();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && truck != null)
        {
            truck.OnPlayerExit();
        }
    }
}

// Billboard untuk world-space text
public class BillboardText : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }
    }
}
