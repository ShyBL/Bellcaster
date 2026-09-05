using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// Owns which items are currently equipped on Nina's Toolbelt and tells the
/// matching ToolbeltAttachmentPoint to spawn/update its visual. Mirrors
/// InventoryManager's shape (singleton, simple dictionary, change event) —
/// no visual/attachment logic lives here beyond delegating to the
/// attachment points themselves.
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    [Header("Pack")]
    public List<InteractableData> PackItems = new List<InteractableData>();
 
    [Header("Souvenirs")]
    [Tooltip("Souvenirs Nina has actually found. Cross-referenced against a SouvenirRegistry by JournalPanelController to render locked/unlocked slots.")]
    public List<InteractableData> CollectedSouvenirs = new List<InteractableData>();
    

    [Header("Attachment Points")]
    [SerializeField, Tooltip("One entry per socket this Toolbelt supports. Each must already have a Spine BoneFollower configured on it.")]
    private List<ToolbeltAttachmentPoint> _attachmentPoints = new List<ToolbeltAttachmentPoint>();
    
    private readonly Dictionary<ToolbeltSocket, InteractableData> _equippedItems = new Dictionary<ToolbeltSocket, InteractableData>();
    private Dictionary<ToolbeltSocket, ToolbeltAttachmentPoint> _pointLookup;
        
    [Header("View")]
    [SerializeField, Tooltip("Shared prefab (icon + collider) spawned at whichever attachment point an item equips to.")]
    private ToolbeltItemView _itemViewPrefab;

    /// <summary>Raised whenever PackItems changes — PackPanelController listens to refresh its grid live.</summary>
    public event Action OnPackChanged;
 
    /// <summary>Raised whenever a new souvenir is collected — JournalPanelController listens to refresh its grid live.</summary>
    public event Action OnSouvenirsChanged;
    
    /// <summary>Raised whenever an item is equipped. Nothing currently listens outside the attachment visuals, but it's here for a future HUD hint to hook into.</summary>
    public event Action<ToolbeltSocket, InteractableData> OnToolbeltChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _pointLookup = new Dictionary<ToolbeltSocket, ToolbeltAttachmentPoint>();
        foreach (var point in _attachmentPoints)
        {
            if (point != null) _pointLookup[point.Socket] = point;
        }
    }

    public void Equip(InteractableData item)
    {
        if (item == null) return;

        _equippedItems[item.toolbeltSocket] = item;
        Debug.Log($"[ToolbeltManager] Equipped {item.objectName} at {item.toolbeltSocket}");

        if (_pointLookup.TryGetValue(item.toolbeltSocket, out var point))
        {
            point.Attach(item, _itemViewPrefab);
        }
        else
        {
            Debug.LogWarning($"[ToolbeltManager] No attachment point configured for socket '{item.toolbeltSocket}'.", this);
        }

        OnToolbeltChanged?.Invoke(item.toolbeltSocket, item);
    }

    public bool IsEquipped(string itemName)
    {
        foreach (var item in _equippedItems.Values)
        {
            if (item != null && item.objectName == itemName) return true;
        }
        return false;
    }

    public InteractableData GetEquipped(ToolbeltSocket socket)
    {
        return _equippedItems.TryGetValue(socket, out var item) ? item : null;
    }

    /// <summary>Returns the equipped item view under a world point, or null. Used by PlayerInputHandler to detect the start of a world-space item drag.</summary>
    public ToolbeltItemView GetItemViewAtWorld(Vector2 worldPos)
    {
        foreach (var point in _attachmentPoints)
        {
            if (point == null || point.CurrentView == null) continue;

            if (point.CurrentView.Collider != null && point.CurrentView.Collider.OverlapPoint(worldPos))
                return point.CurrentView;
        }
        return null;
    }
    
    public void AddToPack(InteractableData item)
    {
        PackItems.Add(item);
        Debug.Log($"Added to Pack: {item.objectName}");
        OnPackChanged?.Invoke();
    }
 
    public void AddSouvenir(InteractableData item)
    {
        if (CollectedSouvenirs.Contains(item)) return;
 
        CollectedSouvenirs.Add(item);
        Debug.Log($"Added Souvenir: {item.objectName}");
        OnSouvenirsChanged?.Invoke();
    }
 
    public void AddJournal(InteractableData data)
    {
        
    }
    
    public bool HasSouvenir(InteractableData item)
    {
        return item != null && CollectedSouvenirs.Contains(item);
    }
 
    public bool HasItem(string itemName)
    {
        return PackItems.Exists(item => item.objectName == itemName);
    }
 
    public void RemoveItem(string itemName)
    {
        var item = PackItems.Find(i => i.objectName == itemName);
        if (item != null)
        {
            PackItems.Remove(item);
            Debug.Log($"Removed from Pack: {itemName}");
            OnPackChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Tried to remove item '{itemName}' but it was not found.");
        }
    }


}