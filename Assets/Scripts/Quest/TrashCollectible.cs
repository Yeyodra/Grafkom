using UnityEngine;

public class TrashCollectible : MonoBehaviour
{
    [Header("Settings")]
    public string questId = "collect_trash";
    public float collectRadius = 1.5f;
    
    [Header("Visual")]
    public GameObject floatingIndicator;
    
    private bool isCollected = false;
    private InventoryUI inventoryUI;
    
    void Start()
    {
        // Setup collider sebagai trigger
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }
        col.isTrigger = true;
        col.radius = collectRadius;
        
        // Cari InventoryUI untuk animasi
        inventoryUI = FindFirstObjectByType<InventoryUI>();
        
        // Setup floating indicator jika belum ada
        if (floatingIndicator == null)
        {
            CreateFloatingIndicator();
        }
    }
    
    void CreateFloatingIndicator()
    {
        // Buat simple floating indicator
        floatingIndicator = new GameObject("FloatingIndicator");
        floatingIndicator.transform.SetParent(transform);
        floatingIndicator.transform.localPosition = new Vector3(0, 2f, 0);
        
        // Add FloatingIndicator component jika ada
        FloatingIndicator indicator = floatingIndicator.AddComponent<FloatingIndicator>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        // Check apakah player
        if (!other.CompareTag("Player")) return;
        
        TryCollect();
    }
    
    public void TryCollect()
    {
        if (isCollected) return;
        
        // Check inventory
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager tidak ditemukan!");
            return;
        }
        
        if (InventoryManager.Instance.IsFull())
        {
            Debug.Log("Inventory penuh! Buang sampah ke mobil dulu.");
            return;
        }
        
        // Collect!
        isCollected = true;
        
        // Add ke inventory
        InventoryManager.Instance.AddItem("trash");
        
        // Trigger slot animation
        if (inventoryUI != null)
        {
            int slotIndex = InventoryManager.Instance.GetCount() - 1;
            
            // Start fly animation lalu destroy
            StartCoroutine(FlyToInventory(slotIndex));
        }
        else
        {
            // Langsung destroy jika tidak ada UI
            Destroy(gameObject);
        }
    }
    
    System.Collections.IEnumerator FlyToInventory(int slotIndex)
    {
        // Hide indicator
        if (floatingIndicator != null)
            floatingIndicator.SetActive(false);
        
        // Get target position (screen position of slot)
        Vector3 startPos = transform.position;
        Vector3 endPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        
        if (inventoryUI != null)
        {
            // Convert slot screen position ke world position untuk target
            Vector3 slotScreenPos = inventoryUI.GetSlotWorldPosition(slotIndex);
            endPos = Camera.main.ScreenToWorldPoint(new Vector3(slotScreenPos.x, slotScreenPos.y, 2f));
        }
        
        // Shrink dan fly
        float duration = 0.4f;
        float elapsed = 0;
        Vector3 originalScale = transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease out curve
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            // Move towards camera/inventory
            transform.position = Vector3.Lerp(startPos, endPos, easeT);
            
            // Shrink
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, easeT);
            
            yield return null;
        }
        
        // Trigger slot pop animation
        if (inventoryUI != null)
        {
            inventoryUI.AnimateSlot(slotIndex);
        }
        
        // Destroy object
        Destroy(gameObject);
    }
}
