using UnityEngine;
using UnityEngine.InputSystem;

public enum NPCRole { QuestGiver, Checkpoint, QuestEnder, QuestGiverAndEnder, Inactive }

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName = "Stranger";
    public Color nameColor = Color.yellow;
    public NPCRole role = NPCRole.QuestGiver;
    
    [Header("Quest Settings")]
    public string questId = "delivery_medicine";
    public int requiredQuestStage = 0; // Stage saat NPC ini aktif
    
    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    
    [Header("Indicator")]
    [Header("Dialogue Saat Deliver (untuk QuestGiverAndEnder)")]
    [TextArea(2, 5)]
    public string[] deliveryDialogueLines;
    
    
public NPCIndicator.IndicatorType indicatorWhenActive = NPCIndicator.IndicatorType.Exclamation;
    
    [Header("Interaction")]
    public float interactionRadius = 3f;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("References")]
    public NPCIndicator indicator;
    
    private bool playerInRange = false;
    private bool hasInteracted = false;
    private Transform player;
    private SphereCollider triggerCollider;
    
    // Events
    public System.Action onDialogueStart;
    public System.Action onDialogueEnd;
    
    void Start()
    {
        SetupCollider();
        SetupIndicator();
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
    
    void SetupCollider()
    {
        triggerCollider = GetComponent<SphereCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider>();
        }
        triggerCollider.isTrigger = true;
        triggerCollider.radius = interactionRadius;
    }
    
    void SetupIndicator()
    {
        if (indicator == null)
        {
            indicator = GetComponent<NPCIndicator>();
            if (indicator == null)
            {
                indicator = gameObject.AddComponent<NPCIndicator>();
            }
        }
        
        UpdateIndicatorState();
    }
    
void Update()
    {
        if (!playerInRange) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive()) return;
        
        // Check quest stage
        if (!IsActiveForCurrentStage()) return;
        
        // Show prompt jika belum ada
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowPrompt("Tekan E untuk bicara");
        }
        
        // New Input System compatible
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartDialogue();
        }
    }
    
bool IsActiveForCurrentStage()
    {
        if (QuestManager.Instance == null) return true;
        
        Quest activeQuest = QuestManager.Instance.GetActiveQuest();
        if (activeQuest == null) return false;
        if (activeQuest.questId != questId) return false;
        
        bool hasItem = QuestManager.Instance.carryingDeliveryItem;
        
        // Check stage based on role
        switch (role)
        {
            case NPCRole.QuestGiver:
                return !hasItem && !hasInteracted;
            case NPCRole.Checkpoint:
                return !hasItem && !hasInteracted;
            case NPCRole.QuestEnder:
                return hasItem;
            case NPCRole.QuestGiverAndEnder:
                // Aktif di awal (kasih quest) ATAU di akhir (terima delivery)
                if (!hasInteracted && !hasItem)
                    return true; // Awal - belum interact, belum punya item
                if (hasItem)
                    return true; // Akhir - punya item, bisa deliver
                return false;
            case NPCRole.Inactive:
                return false;
        }
        return false;
    }
    
void StartDialogue()
    {
        if (DialogueManager.Instance == null) return;
        
        // Pilih dialogue berdasarkan state
        string[] linesToUse = dialogueLines;
        
        if (role == NPCRole.QuestGiverAndEnder && QuestManager.Instance != null)
        {
            if (QuestManager.Instance.carryingDeliveryItem && deliveryDialogueLines != null && deliveryDialogueLines.Length > 0)
            {
                linesToUse = deliveryDialogueLines;
            }
        }
        
        if (linesToUse == null || linesToUse.Length == 0) return;
        
        // Untuk QuestGiverAndEnder, jangan set hasInteracted di sini
        if (role != NPCRole.QuestGiverAndEnder)
        {
            hasInteracted = true;
        }
        
        onDialogueStart?.Invoke();
        
        // Hide indicator
        if (indicator != null)
            indicator.Hide();
        
        // Hide prompt
        DialogueManager.Instance.HidePrompt();
        
        // Start dialogue
        DialogueManager.Instance.StartDialogue(npcName, linesToUse, nameColor, OnDialogueComplete);
    }
    
void OnDialogueComplete()
    {
        onDialogueEnd?.Invoke();
        
        // Trigger quest actions based on role
        if (QuestManager.Instance != null)
        {
            switch (role)
            {
                case NPCRole.QuestGiver:
                    Debug.Log($"[NPCDialogue] Quest giver interaction complete: {npcName}");
                    break;
                    
                case NPCRole.Checkpoint:
                    QuestManager.Instance.OnPickupItem(questId);
                    Debug.Log($"[NPCDialogue] Checkpoint interaction - item picked up: {npcName}");
                    break;
                    
                case NPCRole.QuestEnder:
                    Debug.Log($"[NPCDialogue] Quest ender interaction: {npcName}");
                    QuestManager.Instance.TriggerDeliveryCutscene();
                    break;
                    
                case NPCRole.QuestGiverAndEnder:
                    if (QuestManager.Instance.carryingDeliveryItem)
                    {
                        // Akhir - terima delivery, trigger cutscene
                        Debug.Log($"[NPCDialogue] Quest ender interaction: {npcName}");
                        QuestManager.Instance.TriggerDeliveryCutscene();
                    }
                    else
                    {
                        // Awal - kasih quest
                        Debug.Log($"[NPCDialogue] Quest giver interaction complete: {npcName}");
                        hasInteracted = true;
                    }
                    break;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        playerInRange = true;
        
        if (!hasInteracted && IsActiveForCurrentStage())
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowPrompt("Tekan E untuk bicara");
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        playerInRange = false;
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.HidePrompt();
        }
    }
    
    public void UpdateIndicatorState()
    {
        if (indicator == null) return;
        
        if (hasInteracted || role == NPCRole.Inactive)
        {
            indicator.UpdateIndicatorType(NPCIndicator.IndicatorType.None);
        }
        else if (IsActiveForCurrentStage())
        {
            indicator.UpdateIndicatorType(indicatorWhenActive);
        }
        else
        {
            indicator.UpdateIndicatorType(NPCIndicator.IndicatorType.None);
        }
    }
    
    public void ResetInteraction()
    {
        hasInteracted = false;
        UpdateIndicatorState();
    }
    
    public bool HasInteracted => hasInteracted;
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
