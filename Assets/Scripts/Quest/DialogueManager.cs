using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Text npcNameText;
    public CanvasGroup canvasGroup;
    
    [Header("Settings")]
    public float fadeSpeed = 3f;
    public float displayDuration = 3f;
    public float characterDelay = 0.03f;
    
    [Header("Prompt UI")]
    public GameObject interactPrompt;
    public Text promptText;
    
    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    private bool isDisplaying = false;
    private System.Action onDialogueComplete;
    
    [System.Serializable]
    public class DialogueLine
    {
        public string npcName;
        public string text;
        public Color nameColor = Color.yellow;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        SetupUI();
        HideDialogue();
        HidePrompt();
    }
    
    void SetupUI()
    {
        if (dialoguePanel == null)
        {
            // Create dialogue canvas
            GameObject canvasObj = new GameObject("DialogueCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Dialogue panel - bottom center
            dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(canvasObj.transform);
            RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 80);
            panelRect.sizeDelta = new Vector2(800, 120);
            
            Image panelImage = dialoguePanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
            
            // NPC Name text
            GameObject nameObj = new GameObject("NPCName");
            nameObj.transform.SetParent(dialoguePanel.transform);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.pivot = new Vector2(0.5f, 1);
            nameRect.anchoredPosition = new Vector2(0, -10);
            nameRect.sizeDelta = new Vector2(-40, 30);
            
            npcNameText = nameObj.AddComponent<Text>();
            npcNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            npcNameText.fontSize = 22;
            npcNameText.fontStyle = FontStyle.Bold;
            npcNameText.alignment = TextAnchor.MiddleLeft;
            npcNameText.color = Color.yellow;
            
            // Dialogue text
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(dialoguePanel.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0, -10);
            textRect.sizeDelta = new Vector2(-40, -50);
            
            dialogueText = textObj.AddComponent<Text>();
            dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dialogueText.fontSize = 20;
            dialogueText.alignment = TextAnchor.MiddleCenter;
            dialogueText.color = Color.white;
            
            // Interact prompt - bottom center above dialogue
            interactPrompt = new GameObject("InteractPrompt");
            interactPrompt.transform.SetParent(canvasObj.transform);
            RectTransform promptRect = interactPrompt.AddComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.5f, 0);
            promptRect.anchorMax = new Vector2(0.5f, 0);
            promptRect.pivot = new Vector2(0.5f, 0);
            promptRect.anchoredPosition = new Vector2(0, 220);
            promptRect.sizeDelta = new Vector2(300, 50);
            
            Image promptBg = interactPrompt.AddComponent<Image>();
            promptBg.color = new Color(0, 0, 0, 0.7f);
            
            GameObject promptTextObj = new GameObject("PromptText");
            promptTextObj.transform.SetParent(interactPrompt.transform);
            RectTransform ptRect = promptTextObj.AddComponent<RectTransform>();
            ptRect.anchorMin = Vector2.zero;
            ptRect.anchorMax = Vector2.one;
            ptRect.sizeDelta = Vector2.zero;
            ptRect.anchoredPosition = Vector2.zero;
            
            promptText = promptTextObj.AddComponent<Text>();
            promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            promptText.fontSize = 24;
            promptText.alignment = TextAnchor.MiddleCenter;
            promptText.color = Color.white;
            promptText.text = "Tekan E untuk bicara";
        }
    }
    
    public void ShowPrompt(string text = "Tekan E untuk bicara")
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(true);
            if (promptText != null)
                promptText.text = text;
        }
    }
    
    public void HidePrompt()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }
    
    public void StartDialogue(DialogueLine[] lines, System.Action onComplete = null)
    {
        dialogueQueue.Clear();
        foreach (var line in lines)
        {
            dialogueQueue.Enqueue(line);
        }
        onDialogueComplete = onComplete;
        
        HidePrompt();
        DisplayNextLine();
    }
    
    public void StartDialogue(string npcName, string[] lines, Color nameColor, System.Action onComplete = null)
    {
        DialogueLine[] dialogueLines = new DialogueLine[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            dialogueLines[i] = new DialogueLine
            {
                npcName = npcName,
                text = lines[i],
                nameColor = nameColor
            };
        }
        StartDialogue(dialogueLines, onComplete);
    }
    
    void DisplayNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            StartCoroutine(FadeOut());
            onDialogueComplete?.Invoke();
            return;
        }
        
        DialogueLine line = dialogueQueue.Dequeue();
        StartCoroutine(DisplayLine(line));
    }
    
IEnumerator DisplayLine(DialogueLine line)
    {
        isDisplaying = true;
        
        if (npcNameText != null)
        {
            npcNameText.text = line.npcName;
            npcNameText.color = line.nameColor;
        }
        
        if (dialogueText != null)
            dialogueText.text = "";
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Typewriter effect
        if (dialogueText != null)
        {
            foreach (char c in line.text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(characterDelay);
            }
        }
        
        // Wait for display duration or player input (New Input System)
        float timer = 0;
        while (timer < displayDuration)
        {
            if (Keyboard.current != null && 
                (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                break;
            timer += Time.deltaTime;
            yield return null;
        }
        
        isDisplaying = false;
        DisplayNextLine();
    }
    
    IEnumerator FadeIn()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
    }
    
    IEnumerator FadeOut()
    {
        if (canvasGroup != null)
        {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 0;
        }
        
        HideDialogue();
    }
    
    void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public bool IsDialogueActive()
    {
        return isDisplaying || dialogueQueue.Count > 0;
    }
}
