using UnityEngine;

public class InteractableData : ScriptableObject
{
    [Header("Basic Info")]
    public string objectName;
    public Sprite objectIcon;
    
    [Header("Examine")]
    public bool canExamine = true;
    [TextArea(3, 6)]
    public string examineText;
    public AudioClip examineVO;
    
    [Header("Pick Up")]
    public bool canPickUp = false;
    public PickupDestination pickupDestination = PickupDestination.Inventory;
    [Tooltip("Required world state boolean (e.g., 'chairInPosition')")]
    public string pickupRequirement = "";
    
    [Header("Interact")]
    public bool canInteract = false;
    [Tooltip("Item name required in inventory (leave empty if none)")]
    public string requiredInventoryItem = "";
    public Sprite requiredItemIcon; // Show this if item is missing
    [Tooltip("World state boolean this sets when used (e.g., 'doorbellFixed')")]
    public string interactResultState = "";
    [Tooltip("GameObject to activate after interaction (optional)")]
    public GameObject interactResultObject;
    
    [Header("Visual Changes")]
    public Sprite spriteAfterInteract;
    public GameObject vfxPrefab; // Particle effect or animation
}