using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public List<Image> slotImages = new List<Image>();
    public List<Image> slotIcons = new List<Image>();
    
    [Header("Settings")]
    public Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color filledSlotColor = new Color(0.3f, 0.5f, 0.3f, 0.9f);
    public Sprite trashIcon;
    
    private bool isSetup = false;
    
    void Start()
    {
        SetupUI();
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateUI;
        }
        
        UpdateUI();
    }
    
    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateUI;
        }
    }
    
    void SetupUI()
    {
        if (isSetup) return;
        
        if (inventoryPanel == null)
        {
            // Create inventory canvas
            GameObject canvasObj = new GameObject("InventoryCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create inventory panel - center bottom
            inventoryPanel = new GameObject("InventoryPanel");
            inventoryPanel.transform.SetParent(canvasObj.transform);
            
            RectTransform panelRect = inventoryPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 20);
            panelRect.sizeDelta = new Vector2(180, 80);
            
            // Panel background
            Image panelBg = inventoryPanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.5f);
            
            // Horizontal layout
            HorizontalLayoutGroup layout = inventoryPanel.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            
            // Create 2 slots
            for (int i = 0; i < 2; i++)
            {
                GameObject slot = CreateSlot(i);
                slot.transform.SetParent(inventoryPanel.transform);
            }
        }
        
        isSetup = true;
        
        // Inventory selalu visible sebagai bagian dari game UI
        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }
    
    GameObject CreateSlot(int index)
    {
        // Slot container
        GameObject slot = new GameObject($"Slot_{index}");
        RectTransform slotRect = slot.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(60, 60);
        
        // Slot background
        Image slotBg = slot.AddComponent<Image>();
        slotBg.color = emptySlotColor;
        slotImages.Add(slotBg);
        
        // Icon child
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slot.transform);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-10, -10);
        iconRect.anchoredPosition = Vector2.zero;
        
        Image icon = iconObj.AddComponent<Image>();
        icon.color = Color.white;
        icon.enabled = false;
        slotIcons.Add(icon);
        
        return slot;
    }
    
    public void UpdateUI()
    {
        if (InventoryManager.Instance == null) return;
        
        int itemCount = InventoryManager.Instance.GetCount();
        
        for (int i = 0; i < slotImages.Count; i++)
        {
            bool hasItem = i < itemCount;
            
            // Update slot background
            if (slotImages[i] != null)
            {
                slotImages[i].color = hasItem ? filledSlotColor : emptySlotColor;
            }
            
            // Update icon visibility
            if (slotIcons[i] != null)
            {
                slotIcons[i].enabled = hasItem;
                if (hasItem && trashIcon != null)
                {
                    slotIcons[i].sprite = trashIcon;
                }
            }
        }
    }
    
    public void Show()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }
    
    public void Hide()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }
    
    // Get world position of slot untuk fly animation
    public Vector3 GetSlotWorldPosition(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotImages.Count) 
            return Vector3.zero;
        
        return slotImages[slotIndex].transform.position;
    }
    
    // Trigger pop animation pada slot
    public void AnimateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotImages.Count) return;
        
        StartCoroutine(PopAnimation(slotImages[slotIndex].transform));
    }
    
    System.Collections.IEnumerator PopAnimation(Transform slot)
    {
        Vector3 originalScale = slot.localScale;
        Vector3 popScale = originalScale * 1.2f;
        
        // Scale up
        float duration = 0.1f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slot.localScale = Vector3.Lerp(originalScale, popScale, elapsed / duration);
            yield return null;
        }
        
        // Scale down
        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slot.localScale = Vector3.Lerp(popScale, originalScale, elapsed / duration);
            yield return null;
        }
        
        slot.localScale = originalScale;
    }
}
