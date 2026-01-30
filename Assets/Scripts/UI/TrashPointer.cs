using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TrashPointer : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform arrowImage;
    public Text distanceText;
    
    [Header("Settings")]
    public float edgePadding = 50f;
    public Color arrowColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;
    
    private Camera mainCamera;
    private Canvas canvas;
    private RectTransform canvasRect;
    private bool isSetup = false;
    
    void Start()
    {
        mainCamera = Camera.main;
        SetupUI();
    }
    
    void SetupUI()
    {
        if (isSetup) return;
        
        // Create canvas
        GameObject canvasObj = new GameObject("TrashPointerCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        canvasRect = canvasObj.GetComponent<RectTransform>();
        
        // Create arrow container (center of screen)
        GameObject arrowContainer = new GameObject("ArrowContainer");
        arrowContainer.transform.SetParent(canvasObj.transform);
        RectTransform containerRect = arrowContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(100, 100);
        
        // Create arrow using UI elements (triangle shape)
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.SetParent(arrowContainer.transform);
        arrowImage = arrow.AddComponent<RectTransform>();
        arrowImage.anchorMin = new Vector2(0.5f, 0.5f);
        arrowImage.anchorMax = new Vector2(0.5f, 0.5f);
        arrowImage.sizeDelta = new Vector2(60, 60);
        arrowImage.anchoredPosition = new Vector2(0, 150); // Offset from center
        
        // Add arrow visual (using Text as simple arrow)
        Text arrowText = arrow.AddComponent<Text>();
        arrowText.text = "^"; // Simple arrow character
        arrowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        arrowText.fontSize = 72;
        arrowText.fontStyle = FontStyle.Bold;
        arrowText.alignment = TextAnchor.MiddleCenter;
        arrowText.color = arrowColor;
        
        // Add outline for visibility
        Outline outline = arrow.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
        
        // Create distance text
        GameObject distObj = new GameObject("DistanceText");
        distObj.transform.SetParent(arrowContainer.transform);
        RectTransform distRect = distObj.AddComponent<RectTransform>();
        distRect.anchorMin = new Vector2(0.5f, 0.5f);
        distRect.anchorMax = new Vector2(0.5f, 0.5f);
        distRect.sizeDelta = new Vector2(200, 40);
        distRect.anchoredPosition = new Vector2(0, 100);
        
        distanceText = distObj.AddComponent<Text>();
        distanceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        distanceText.fontSize = 24;
        distanceText.alignment = TextAnchor.MiddleCenter;
        distanceText.color = Color.white;
        
        Outline distOutline = distObj.AddComponent<Outline>();
        distOutline.effectColor = Color.black;
        distOutline.effectDistance = new Vector2(1, 1);
        
        isSetup = true;
    }
    
    void Update()
    {
        if (!isSetup || mainCamera == null) return;
        
        // Only show during Quest 2
        if (QuestManager.Instance == null)
        {
            HidePointer();
            return;
        }
        
        var quest = QuestManager.Instance.GetActiveQuest();
        if (quest == null || quest.questId != "collect_trash" || quest.state != QuestState.Active)
        {
            HidePointer();
            return;
        }
        
        // Find nearest trash
        Transform nearestTrash = FindNearestTrash();
        
        if (nearestTrash == null)
        {
            HidePointer();
            return;
        }
        
        ShowPointer();
        UpdatePointer(nearestTrash);
    }
    
    Transform FindNearestTrash()
    {
        // Find all collectibles with collect_trash quest
        Collectible[] collectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        
        Transform nearest = null;
        float nearestDist = float.MaxValue;
        Vector3 playerPos = mainCamera.transform.position;
        
        foreach (var collectible in collectibles)
        {
            if (collectible.questId != "collect_trash") continue;
            
            float dist = Vector3.Distance(playerPos, collectible.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = collectible.transform;
            }
        }
        
        return nearest;
    }
    
    void UpdatePointer(Transform target)
    {
        if (arrowImage == null || target == null) return;
        
        Vector3 playerPos = mainCamera.transform.position;
        Vector3 targetPos = target.position;
        
        // Calculate direction to target
        Vector3 dirToTarget = targetPos - playerPos;
        dirToTarget.y = 0; // Ignore height difference
        
        // Get camera forward (ignore Y)
        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();
        
        // Calculate angle between camera forward and target direction
        float angle = Vector3.SignedAngle(camForward, dirToTarget.normalized, Vector3.up);
        
        // Rotate arrow to point at target
        arrowImage.localRotation = Quaternion.Euler(0, 0, -angle);
        
        // Update distance text
        float distance = dirToTarget.magnitude;
        if (distanceText != null)
        {
            distanceText.text = $"{distance:F0}m";
        }
        
        // Pulse effect
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        arrowImage.localScale = Vector3.one * pulse;
        
        // Change color based on distance (closer = more green)
        Text arrowText = arrowImage.GetComponent<Text>();
        if (arrowText != null)
        {
            if (distance < 10f)
                arrowText.color = Color.green;
            else if (distance < 25f)
                arrowText.color = Color.yellow;
            else
                arrowText.color = new Color(1f, 0.5f, 0f); // Orange
        }
    }
    
    void ShowPointer()
    {
        if (arrowImage != null)
            arrowImage.gameObject.SetActive(true);
        if (distanceText != null)
            distanceText.gameObject.SetActive(true);
    }
    
    void HidePointer()
    {
        if (arrowImage != null)
            arrowImage.gameObject.SetActive(false);
        if (distanceText != null)
            distanceText.gameObject.SetActive(false);
    }
}
