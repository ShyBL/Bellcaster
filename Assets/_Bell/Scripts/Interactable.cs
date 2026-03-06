using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Interactable : MonoBehaviour
{
    public InteractableData data;
    
    private SpriteRenderer spriteRenderer;
    private bool hasBeenInteracted = false;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void OnMouseDown()
    {
        if (data == null)
        {
            Debug.LogWarning($"No InteractableData assigned to {gameObject.name}");
            return;
        }
        
        List<InteractionType> availableInteractions = GetAvailableInteractions();
        InteractionMenu.Instance.ShowMenu(this, transform.position, availableInteractions);
    }
    
    public List<InteractionType> GetAvailableInteractions()
    {
        List<InteractionType> interactions = new List<InteractionType>();
        
        if (data.canExamine)
        {
            interactions.Add(InteractionType.Examine);
        }
        
        if (data.canPickUp && !hasBeenInteracted)
        {
            // Check if pickup requirement is met
            if (WorldState.Instance.CheckRequirement(data.pickupRequirement))
            {
                interactions.Add(InteractionType.PickUp);
            }
        }
        
        if (data.canInteract && !hasBeenInteracted)
        {
            interactions.Add(InteractionType.Interact);
        }
        
        return interactions;
    }
    
    public bool CanInteract()
    {
        // Check if we have required item
        if (!string.IsNullOrEmpty(data.requiredInventoryItem))
        {
            return InventoryManager.Instance.HasItem(data.requiredInventoryItem);
        }
        return true;
    }
    
    public void OnExamine()
    {
        Debug.Log($"[EXAMINE] {data.objectName}: {data.examineText}");
        
        // Show text with animation
        ExamineTextDisplay.Instance.ShowText(data.examineText);
        
        if (data.examineVO != null)
        {
            // Play audio
            AudioSource.PlayClipAtPoint(data.examineVO, Camera.main.transform.position);
        }
    }
    
    public void OnPickUp()
    {
        if (!WorldState.Instance.CheckRequirement(data.pickupRequirement))
        {
            Debug.Log($"Cannot pick up {data.objectName}: requirement '{data.pickupRequirement}' not met");
            return;
        }
        
        Debug.Log($"[PICK UP] {data.objectName}");
        
        if (data.pickupDestination == PickupDestination.Inventory)
        {
            InventoryManager.Instance.AddToInventory(data.objectName);
        }
        else if (data.pickupDestination == PickupDestination.Journal)
        {
            InventoryManager.Instance.AddToJournal(data.objectName);
        }
        
        hasBeenInteracted = true;
        gameObject.SetActive(false); // Remove from scene
    }
    
    public void OnInteract()
    {
        // Check if we have required item
        if (!string.IsNullOrEmpty(data.requiredInventoryItem))
        {
            if (!InventoryManager.Instance.HasItem(data.requiredInventoryItem))
            {
                Debug.Log($"Need item: {data.requiredInventoryItem}");
                // TODO: Show required item icon (data.requiredItemIcon)
                return;
            }
            
            // Remove item from inventory after use
            InventoryManager.Instance.RemoveItem(data.requiredInventoryItem);
        }
        
        Debug.Log($"[INTERACT] {data.objectName}");
        
        // Set world state
        if (!string.IsNullOrEmpty(data.interactResultState))
        {
            WorldState.Instance.SetState(data.interactResultState, true);
        }
        
        // Change sprite
        if (data.spriteAfterInteract != null)
        {
            spriteRenderer.sprite = data.spriteAfterInteract;
        }
        
        // Spawn VFX
        if (data.vfxPrefab != null)
        {
            Instantiate(data.vfxPrefab, transform.position, Quaternion.identity);
        }
        
        // Activate result object
        if (data.interactResultObject != null)
        {
            data.interactResultObject.SetActive(true);
        }
        
        hasBeenInteracted = true;
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Create New Data")]
    void CreateNewData()
    {
        string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
            "Create Interactable Data",
            gameObject.name + "_Data",
            "asset",
            "Create new InteractableData"
        );
    
        if (!string.IsNullOrEmpty(path))
        {
            InteractableData newData = ScriptableObject.CreateInstance<InteractableData>();
            newData.objectName = gameObject.name;
            UnityEditor.AssetDatabase.CreateAsset(newData, path);
            UnityEditor.AssetDatabase.SaveAssets();
            data = newData;
        }
    }
    #endif
}