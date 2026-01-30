using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Auto-setup script untuk menghubungkan UI elements ke QuestManager.
/// Attach script ini ke QuestCanvas, lalu akan auto-assign saat Start().
/// </summary>
public class QuestUISetup : MonoBehaviour
{
    void Start()
    {
        SetupQuestUI();
        StyleUI();
    }
    
    [ContextMenu("Setup Quest UI")]
    public void SetupQuestUI()
    {
        QuestManager questManager = FindFirstObjectByType<QuestManager>();
        if (questManager == null)
        {
            Debug.LogError("QuestManager not found in scene!");
            return;
        }
        
        // Find QuestPanel
        Transform questPanel = transform.Find("QuestPanel");
        if (questPanel != null)
        {
            questManager.questPanel = questPanel.gameObject;
            
            // Find title text
            Transform titleText = questPanel.Find("QuestTitleText");
            if (titleText != null)
            {
                questManager.questTitleText = titleText.GetComponent<Text>();
            }
            
            // Find progress text
            Transform progressText = questPanel.Find("QuestProgressText");
            if (progressText != null)
            {
                questManager.questProgressText = progressText.GetComponent<Text>();
            }
        }
        
        // Find QuestCompletePanel
        Transform completePanel = transform.Find("QuestCompletePanel");
        if (completePanel != null)
        {
            questManager.questCompletePanel = completePanel.gameObject;
            
            // Find complete text
            Transform completeText = completePanel.Find("QuestCompleteText");
            if (completeText != null)
            {
                questManager.questCompleteText = completeText.GetComponent<Text>();
            }
        }
        
        Debug.Log("Quest UI setup complete!");
    }
    
    void StyleUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        
        // Style QuestPanel - top left
        Transform questPanel = transform.Find("QuestPanel");
        if (questPanel != null)
        {
            RectTransform panelRect = questPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(20, -20);
            panelRect.sizeDelta = new Vector2(350, 100);
            
            Image panelImage = questPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            }
            
            // Style title text
            Transform titleText = questPanel.Find("QuestTitleText");
            if (titleText != null)
            {
                RectTransform titleRect = titleText.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 1);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.pivot = new Vector2(0.5f, 1);
                titleRect.anchoredPosition = new Vector2(0, -10);
                titleRect.sizeDelta = new Vector2(-20, 35);
                
                Text titleTxt = titleText.GetComponent<Text>();
                if (titleTxt != null)
                {
                    titleTxt.fontSize = 22;
                    titleTxt.fontStyle = FontStyle.Bold;
                    titleTxt.color = Color.white;
                    titleTxt.alignment = TextAnchor.MiddleLeft;
                }
            }
            
            // Style progress text
            Transform progressText = questPanel.Find("QuestProgressText");
            if (progressText != null)
            {
                RectTransform progressRect = progressText.GetComponent<RectTransform>();
                progressRect.anchorMin = new Vector2(0, 0);
                progressRect.anchorMax = new Vector2(1, 1);
                progressRect.pivot = new Vector2(0.5f, 0.5f);
                progressRect.anchoredPosition = new Vector2(0, -15);
                progressRect.sizeDelta = new Vector2(-20, -50);
                
                Text progressTxt = progressText.GetComponent<Text>();
                if (progressTxt != null)
                {
                    progressTxt.fontSize = 16;
                    progressTxt.color = new Color(1f, 1f, 0.7f, 1f);
                    progressTxt.alignment = TextAnchor.MiddleLeft;
                }
            }
        }
        
        // Style QuestCompletePanel - center
        Transform completePanel = transform.Find("QuestCompletePanel");
        if (completePanel != null)
        {
            RectTransform completeRect = completePanel.GetComponent<RectTransform>();
            completeRect.anchorMin = new Vector2(0.5f, 0.5f);
            completeRect.anchorMax = new Vector2(0.5f, 0.5f);
            completeRect.pivot = new Vector2(0.5f, 0.5f);
            completeRect.anchoredPosition = Vector2.zero;
            completeRect.sizeDelta = new Vector2(400, 120);
            
            Image completeImage = completePanel.GetComponent<Image>();
            if (completeImage != null)
            {
                completeImage.color = new Color(0.1f, 0.6f, 0.1f, 0.9f);
            }
            
            // Style complete text
            Transform completeText = completePanel.Find("QuestCompleteText");
            if (completeText != null)
            {
                RectTransform textRect = completeText.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = new Vector2(-20, -20);
                
                Text completeTxt = completeText.GetComponent<Text>();
                if (completeTxt != null)
                {
                    completeTxt.fontSize = 28;
                    completeTxt.fontStyle = FontStyle.Bold;
                    completeTxt.color = Color.white;
                    completeTxt.alignment = TextAnchor.MiddleCenter;
                }
            }
            
            // Hide complete panel by default
            completePanel.gameObject.SetActive(false);
        }
        
        Debug.Log("Quest UI styled!");
    }
}
