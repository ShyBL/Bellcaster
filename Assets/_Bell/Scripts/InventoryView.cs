using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    public GameObject slotPrefab;        // Your InventorySlotPrefab
    public Transform gridParent;         // The Grid Layout Group parent

    public static InventoryView Instance;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Clear old slots
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        // Create new slots
        foreach (string itemName in InventoryManager.Instance.inventory)
        {
            GameObject slot = Instantiate(slotPrefab, gridParent);
            
        }
    }
}