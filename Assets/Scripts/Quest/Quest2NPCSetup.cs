using UnityEngine;

/// <summary>
/// Attach ke NPC_PetugasKebersihan untuk auto-setup dialog Quest 2
/// </summary>
public class Quest2NPCSetup : MonoBehaviour
{
    void Start()
    {
        SetupDialogue();
    }
    
    [ContextMenu("Setup Quest 2 NPC Dialogue")]
    public void SetupDialogue()
    {
        NPCDialogue dialogue = GetComponent<NPCDialogue>();
        if (dialogue == null)
        {
            Debug.LogError("NPCDialogue component not found!");
            return;
        }
        
        // Setup NPC info
        dialogue.npcName = "Petugas Kebersihan";
        dialogue.nameColor = new Color(0.2f, 0.8f, 0.2f); // Hijau
        dialogue.questId = "collect_trash";
        dialogue.role = NPCRole.QuestGiver;
        
        // Setup opening dialogue (4 lines)
        dialogue.dialogueLines = new string[]
        {
            "Uhuk uhuk... Aduh...",
            "Saya sudah 3 hari demam, tapi sampah kota numpuk di mana-mana...",
            "Mobilnya juga mogok di sini... uhuk...",
            "Kamu... bisa bantu kumpulkan 5 sampah? Buang ke mobil ini ya..."
        };
        
        // Setup callback untuk start Quest 2
        dialogue.onDialogueEnd += OnDialogueComplete;
        
        Debug.Log("Quest 2 NPC Dialogue setup complete!");
    }
    
    void OnDialogueComplete()
    {
        // Start Quest 2 - spawn trash dan show inventory
        StartQuest2();
    }
    
    void StartQuest2()
    {
        // Show inventory UI
        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.Show();
        }
        
        // Spawn trash
        TrashSpawner spawner = FindFirstObjectByType<TrashSpawner>();
        if (spawner != null)
        {
            spawner.SpawnTrash();
        }
        
        Debug.Log("Quest 2 started - Trash spawned, Inventory shown!");
    }
}
