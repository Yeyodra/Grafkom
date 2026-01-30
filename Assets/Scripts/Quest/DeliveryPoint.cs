using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    [Header("Quest Settings")]
    public string questId = "delivery_medicine";
    public bool isPickupPoint = true; // true = pickup, false = dropoff
    
    [Header("Visual Settings")]
    public Color pickupColor = Color.green;
    public Color dropoffColor = Color.red;
    
    private Renderer pointRenderer;
    private bool isActive = true;
    
    void Start()
    {
        pointRenderer = GetComponent<Renderer>();
        
        // Ensure trigger collider exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 2f;
        }
        else
        {
            col.isTrigger = true;
        }
        
        // Set initial color
        UpdateVisual();
        
        // Dropoff starts hidden
        if (!isPickupPoint)
        {
            SetActive(false);
        }
    }
    
    void Update()
    {
        // Check if this point should be active based on quest state
        if (QuestManager.Instance != null)
        {
            Quest activeQuest = QuestManager.Instance.GetActiveQuest();
            if (activeQuest != null && activeQuest.questId == questId)
            {
                if (isPickupPoint)
                {
                    // Pickup visible when not carrying item
                    SetActive(!QuestManager.Instance.carryingDeliveryItem);
                }
                else
                {
                    // Dropoff visible when carrying item
                    SetActive(QuestManager.Instance.carryingDeliveryItem);
                }
            }
            else
            {
                SetActive(false);
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;
        
        if (QuestManager.Instance == null) return;
        
        if (isPickupPoint)
        {
            QuestManager.Instance.OnPickupItem(questId);
            Debug.Log("Picked up delivery item!");
        }
        else
        {
            QuestManager.Instance.OnDeliverItem(questId);
            Debug.Log("Delivered item!");
        }
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (pointRenderer != null)
        {
            pointRenderer.enabled = active;
        }
        
        // Also toggle collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = active;
        }
    }
    
    void UpdateVisual()
    {
        if (pointRenderer != null)
        {
            // Create material instance to avoid shared material issues
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = isPickupPoint ? pickupColor : dropoffColor;
            
            // Make it emissive so it's visible
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", (isPickupPoint ? pickupColor : dropoffColor) * 2f);
            
            pointRenderer.material = mat;
        }
    }
}
