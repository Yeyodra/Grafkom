using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Quest Settings")]
    public string questId = "collect_trash";
    
    [Header("Visual Settings")]
    public bool rotateObject = true;
    public float rotationSpeed = 50f;
    public bool bobUpDown = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    
    private Vector3 startPosition;
    
void Start()
    {
        startPosition = transform.position;
        
        // Ensure trigger collider exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 1f;
        }
        else
        {
            col.isTrigger = true;
        }
        
        // Register to QuestManager for show/hide
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RegisterQuestObject(questId, gameObject);
        }
    }
    
    void Update()
    {
        // Visual effects
        if (rotateObject)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        if (bobUpDown)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
    
void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (QuestManager.Instance != null)
            {
                bool collected = QuestManager.Instance.OnItemCollected(questId);
                if (collected)
                {
                    Debug.Log($"Collected item for quest: {questId}");
                    Destroy(gameObject);
                }
            }
        }
    }
}
