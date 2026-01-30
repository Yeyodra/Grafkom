using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Settings")]
    public int maxSlots = 2;
    
    [Header("Debug")]
    [SerializeField] private List<string> items = new List<string>();
    
    // Event untuk notify UI saat inventory berubah
    public event Action OnInventoryChanged;
    
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
    
    public bool AddItem(string itemId)
    {
        if (IsFull())
        {
            Debug.Log("Inventory penuh!");
            return false;
        }
        
        items.Add(itemId);
        OnInventoryChanged?.Invoke();
        Debug.Log($"Item ditambahkan: {itemId} ({items.Count}/{maxSlots})");
        return true;
    }
    
    public bool RemoveItem(string itemId)
    {
        if (items.Contains(itemId))
        {
            items.Remove(itemId);
            OnInventoryChanged?.Invoke();
            Debug.Log($"Item dihapus: {itemId} ({items.Count}/{maxSlots})");
            return true;
        }
        return false;
    }
    
    public void ClearItems()
    {
        int count = items.Count;
        items.Clear();
        OnInventoryChanged?.Invoke();
        Debug.Log($"Inventory dikosongkan ({count} items)");
    }
    
    public bool IsFull()
    {
        return items.Count >= maxSlots;
    }
    
    public int GetCount()
    {
        return items.Count;
    }
    
    public List<string> GetItems()
    {
        return new List<string>(items);
    }
    
    public bool HasItems()
    {
        return items.Count > 0;
    }
}
