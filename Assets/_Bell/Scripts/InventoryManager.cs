using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    public List<InteractableData> inventory = new List<InteractableData>();
    public List<InteractableData> journal = new List<InteractableData>();
    
    public GameObject slotPrefab;
    public Transform gridParent;  
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        RefreshUI();
    }
    
    public void AddToInventory(InteractableData item)
    {
        inventory.Add(item);
        Debug.Log($"Added to Inventory: {item.name}");
        RefreshUI();
    }
    
    public void AddToJournal(InteractableData itemName)
    {
        journal.Add(itemName);
        Debug.Log($"Added to Journal: {itemName}");
    }
    
    public bool HasItem(string itemName)
    {
        return inventory.Exists(item => item.objectName == itemName);
    }
    
    public void RemoveItem(string itemName)
    {
        var item = inventory.Find(i => i.objectName == itemName);
        if (item != null)
        {
            inventory.Remove(item);
            Debug.Log($"Removed from Inventory: {itemName}");
        }
        else
        {
            Debug.LogWarning($"Tried to remove item '{itemName}' but it was not found.");
        }
    }

    private void RefreshUI()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (InteractableData inventoryItem in inventory)
        {
            if(gridParent.Find(inventoryItem.objectName) != null)
                continue; // Skip if slot already exists
            
            GameObject slot = Instantiate(slotPrefab, gridParent);
            slot.name = inventoryItem.objectName;
            slot.GetComponent<Image>().sprite = inventoryItem.objectIcon;
        }
    }
}