using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    public List<string> inventory = new List<string>();
    public List<string> journal = new List<string>();
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void AddToInventory(string itemName)
    {
        inventory.Add(itemName);
        Debug.Log($"Added to Inventory: {itemName}");
    }
    
    public void AddToJournal(string itemName)
    {
        journal.Add(itemName);
        Debug.Log($"Added to Journal: {itemName}");
    }
    
    public bool HasItem(string itemName)
    {
        return inventory.Contains(itemName);
    }
    
    public void RemoveItem(string itemName)
    {
        inventory.Remove(itemName);
        Debug.Log($"Removed from Inventory: {itemName}");
    }
}