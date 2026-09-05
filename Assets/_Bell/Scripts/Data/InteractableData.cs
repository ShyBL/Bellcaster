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
    
    [Header("Toolbelt")]
    [Tooltip("Only used when Pickup Destination is Toolbelt — which attachment point this item rides on once equipped.")]
    public ToolbeltSocket toolbeltSocket = ToolbeltSocket.Belt;
    
    [Header("Interact")]
    public bool canInteract = false;
    
    [Tooltip("Item name required in inventory (leave empty if none)")]
    public string requiredInventoryItem = "";
    
    [Header("Wrong Item Response")]
    [Tooltip("What Nina says the first time the wrong item is dropped on this object. Repeat attempts fall back to NinaQuipBank's generic lines instead.")]
    [TextArea(2, 4)]
    public string wrongItemText = "";
    public AudioClip wrongItemVO;
    
    [Tooltip("World state boolean this sets when used (e.g., 'doorbellFixed')")]
    public string interactResultState = "";
    
    [Tooltip("GameObject to instantiate after interaction (optional)")]
    public GameObject interactResultObject;
    
    [Header("Navigate")]
    public bool canNavigate = false;
    [Tooltip("If populated, interacting with this object will navigate to this scene or area key.")]
    public string targetAreaName = "";
    
    [Header("Visual Changes")]
    public Sprite spriteAfterInteract;
    public GameObject vfxPrefab; // Particle effect or animation
    
}