using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum QuestType { Delivery, Collection }
public enum QuestState { Inactive, Active, Completed }

[System.Serializable]
public class Quest
{
    public string questId;
    public string questName;
    public string description;
    public QuestType questType;
    public int targetCount = 1;
    public int currentCount = 0;
    public QuestState state = QuestState.Inactive;
    
    public bool IsCompleted => currentCount >= targetCount;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    [Header("Quests")]
    public List<Quest> quests = new List<Quest>();
    public int currentQuestIndex = 0;
    
    [Header("UI References")]
    public GameObject questPanel;
    public Text questTitleText;
    public Text questProgressText;
    public GameObject questCompletePanel;
    public Text questCompleteText;
    
    [Header("Delivery State")]
    public bool carryingDeliveryItem = false;
    
    [Header("Cutscene")]
    public QuestCutscene questCutscene;

    
    private Quest activeQuest;
    
    // Track all quest objects for show/hide
    private Dictionary<string, List<GameObject>> questObjects = new Dictionary<string, List<GameObject>>();

    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Setup default quests
        if (quests.Count == 0)
        {
            quests.Add(new Quest
            {
                questId = "delivery_medicine",
                questName = "Pengiriman Obat Darurat",
                description = "Ambil paket obat di Rumah Sakit dan antar ke Monas",
                questType = QuestType.Delivery,
                targetCount = 1
            });
            
            quests.Add(new Quest
            {
                questId = "collect_trash",
                questName = "Bersih-bersih Kota",
                description = "Kumpulkan 5 sampah yang tersebar di jalanan kota",
                questType = QuestType.Collection,
                targetCount = 5
            });
        }
        
        if (questCompletePanel != null)
            questCompletePanel.SetActive(false);
            
        // Auto-start first quest
        StartQuest(0);
    }
    
public void StartQuest(int index)
    {
        if (index < 0 || index >= quests.Count) return;
        
        currentQuestIndex = index;
        activeQuest = quests[index];
        activeQuest.state = QuestState.Active;
        activeQuest.currentCount = 0;
        carryingDeliveryItem = false;
        
        // Show/hide quest objects based on active quest
        UpdateQuestObjectsVisibility();
        
        // Update NPC indicators
        UpdateAllNPCIndicators();
        
        UpdateUI();
        Debug.Log($"Quest Started: {activeQuest.questName}");
    }
    
public bool OnItemCollected(string questId)
    {
        if (activeQuest == null || activeQuest.questId != questId) return false;
        if (activeQuest.state != QuestState.Active) return false;
        
        if (activeQuest.questType == QuestType.Collection)
        {
            activeQuest.currentCount++;
            UpdateUI();
            Debug.Log($"Collected: {activeQuest.currentCount}/{activeQuest.targetCount}");
            
            if (activeQuest.IsCompleted)
            {
                CompleteQuest();
            }
        }
        return true;
    }
    
    public void OnPickupItem(string questId)
    {
        if (activeQuest == null || activeQuest.questId != questId) return;
        if (activeQuest.questType != QuestType.Delivery) return;
        
        carryingDeliveryItem = true;
        UpdateUI();
        Debug.Log("Item picked up! Deliver to destination.");
    }
    
    public void OnDeliverItem(string questId)
    {
        if (activeQuest == null || activeQuest.questId != questId) return;
        if (!carryingDeliveryItem) return;
        
        carryingDeliveryItem = false;
        activeQuest.currentCount = 1;
        CompleteQuest();
    }
    
    void CompleteQuest()
    {
        if (activeQuest == null) return;
        
        activeQuest.state = QuestState.Completed;
        Debug.Log($"Quest Completed: {activeQuest.questName}");
        
        // Show completion UI
        if (questCompletePanel != null)
        {
            questCompletePanel.SetActive(true);
            if (questCompleteText != null)
                questCompleteText.text = $"Quest Complete!\n{activeQuest.questName}";
        }
        
        // Start next quest after delay
        Invoke(nameof(StartNextQuest), 3f);
    }
    
    void StartNextQuest()
    {
        if (questCompletePanel != null)
            questCompletePanel.SetActive(false);
            
        int nextIndex = currentQuestIndex + 1;
        if (nextIndex < quests.Count)
        {
            StartQuest(nextIndex);
        }
        else
        {
            // All quests completed
            if (questTitleText != null)
                questTitleText.text = "Semua Quest Selesai!";
            if (questProgressText != null)
                questProgressText.text = "Selamat! Kamu telah menyelesaikan semua quest.";
                
            Debug.Log("All quests completed!");
        }
    }
    
void UpdateUI()
    {
        if (activeQuest == null) return;
        
        if (questPanel != null)
            questPanel.SetActive(true);
            
        if (questTitleText != null)
            questTitleText.text = activeQuest.questName;
            
        if (questProgressText != null)
        {
            if (activeQuest.questType == QuestType.Collection)
            {
                questProgressText.text = $"{activeQuest.currentCount}/{activeQuest.targetCount} terkumpul";
            }
            else if (activeQuest.questType == QuestType.Delivery)
            {
                if (!carryingDeliveryItem)
                    questProgressText.text = "Bicara dengan orang di Monas";
                else
                    questProgressText.text = "Kembali ke Monas dengan obat";
            }
        }
    }
    
    public Quest GetActiveQuest()
    {
        return activeQuest;
    }

public void RegisterQuestObject(string questId, GameObject obj)
    {
        if (!questObjects.ContainsKey(questId))
        {
            questObjects[questId] = new List<GameObject>();
        }
        questObjects[questId].Add(obj);
        
        // Hide if not active quest
        bool shouldShow = activeQuest != null && activeQuest.questId == questId && activeQuest.state == QuestState.Active;
        obj.SetActive(shouldShow);
    }
    
    public void UnregisterQuestObject(string questId, GameObject obj)
    {
        if (questObjects.ContainsKey(questId))
        {
            questObjects[questId].Remove(obj);
        }
    }
    
    void UpdateQuestObjectsVisibility()
    {
        foreach (var kvp in questObjects)
        {
            bool shouldShow = activeQuest != null && activeQuest.questId == kvp.Key && activeQuest.state == QuestState.Active;
            foreach (var obj in kvp.Value)
            {
                if (obj != null)
                    obj.SetActive(shouldShow);
            }
        }
    }

public void TriggerDeliveryCutscene()
    {
        if (questCutscene != null && !questCutscene.IsPlaying)
        {
            questCutscene.PlayCutscene();
        }
        else
        {
            // No cutscene, just complete delivery
            OnDeliverItem("delivery_medicine");
        }
    }
    
    public void UpdateAllNPCIndicators()
    {
        NPCDialogue[] npcs = FindObjectsOfType<NPCDialogue>();
        foreach (var npc in npcs)
        {
            npc.UpdateIndicatorState();
        }
    }


}
