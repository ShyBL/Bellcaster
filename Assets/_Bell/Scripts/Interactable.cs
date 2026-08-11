using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField, Tooltip("Where Nina walks to before the menu opens. " +
                             "Leave empty to use this transform's position.")]
    private Transform _interactionPoint;

    [SerializeField, Tooltip("Outline width in world units")]
    private float _outlineWidth = 0.05f;

    [SerializeField]
    private Color _outlineColor = Color.yellow;

    public PolygonCollider2D _polygonCollider;
    private LineRenderer     _lineRenderer;
    
    public InteractableData data;
    private SpriteRenderer spriteRenderer;
    private bool hasBeenInteracted = false;
    
    #region Helpers

     private List<InteractionType> GetAvailableInteractions()
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
        return hasBeenInteracted;
    }
    
    public bool HasRequiredItem()
    {
        // Check if we have required item
        if (!string.IsNullOrEmpty(data.requiredInventoryItem))
        {
            return InventoryManager.Instance.HasItem(data.requiredInventoryItem);
        }
        return true;
    }
    
    /// <summary>World position Nina should walk toward.</summary>
    public Vector2 InteractionPosition =>
        _interactionPoint != null
            ? (Vector2)_interactionPoint.position
            : (Vector2)transform.position;

    #endregion

    #region Public

    public void OnClick()
    {
        if (hasBeenInteracted) return;
        
        List<InteractionType> availableInteractions = GetAvailableInteractions();
        InteractionMenu.Instance.ShowMenu(this, transform.position, availableInteractions);
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
            InventoryManager.Instance.AddToInventory(data);
        }
        else if (data.pickupDestination == PickupDestination.Journal)
        {
            InventoryManager.Instance.AddToJournal(data);
        }
        
        hasBeenInteracted = true;
        gameObject.SetActive(false); // Remove from scene
    }
    
    public void OnInteract()
    {
        //if (hasBeenInteracted) return;
        
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
           // data.interactResultObject.gameObject.SetActive(true);
           Instantiate(data.interactResultObject, transform.position, Quaternion.identity);
           FindFirstObjectByType<PlayerInputHandler>().RefreshInteractables();
        }
        
        if (!string.IsNullOrEmpty(data.targetAreaName))
        {
            if (NavigationManager.Instance != null)
            {
                NavigationManager.Instance.NavigateTo(data.targetAreaName);
            }
            else
            {
                Debug.LogError("[Interactable] NavigationManager instance not found!");
            }
        }
        
        hasBeenInteracted = true;
    }

    #endregion
    
    #region Unity Lifecycle
    
    private void OnValidate()
    {
        if (data == null)
        {
            Debug.LogWarning($"No InteractableData assigned to {gameObject.name}");
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (_polygonCollider == null)
        {
            _polygonCollider = spriteRenderer.GetComponent<PolygonCollider2D>();
        }

        if (_lineRenderer == null)
        {
            _lineRenderer = spriteRenderer.GetComponent<LineRenderer>();
        }

        if (_lineRenderer != null && _polygonCollider != null)
        {
            ConfigureLineRenderer();
            DrawLine();
        }
        
        if (data != null)
        {
            _hoverLabelText.text = data.objectName;
            _hoverLabelText.gameObject.SetActive(false);
            _hoverLabel.gameObject.SetActive(false);
        }
    }
    
    #endregion

    #region Outline

    /// <summary>Shows or hides the outline highlight.</summary>
    public void SetHighlight(bool highlighted)
    {
        if (_lineRenderer != null)
            _lineRenderer.enabled = highlighted;
    }
    
    // Outline drawing (matches the DrawLine signature in the brief)
    private void ConfigureLineRenderer()
    {
        if (_lineRenderer == null) return;

        _lineRenderer.useWorldSpace    = false; // points are in local space
        _lineRenderer.loop             = true;
        _lineRenderer.startColor       = _outlineColor;
        _lineRenderer.endColor         = _outlineColor;
        _lineRenderer.startWidth       = _outlineWidth;
        _lineRenderer.endWidth         = _outlineWidth;
        _lineRenderer.sortingOrder     = 1;
        _lineRenderer.enabled          = false;
    }
    
    private void DrawLine()
    {
        if (_lineRenderer == null || _polygonCollider == null) return;

        Vector2[] pts = _polygonCollider.points;

        _lineRenderer.positionCount = pts.Length + 1;

        for (int i = 0; i < pts.Length; i++)
            _lineRenderer.SetPosition(i, pts[i]);

        // Close the loop
        _lineRenderer.SetPosition(pts.Length, pts[0]);

        _lineRenderer.startWidth = _outlineWidth;
        _lineRenderer.endWidth   = _outlineWidth;
        _lineRenderer.enabled    = false;
    }
    
    #endregion 
    
    #region Text
    
    [Header("Hover Label")]
    [SerializeField] private TextMeshProUGUI _hoverLabelText;
    [SerializeField] private GameObject _hoverLabel;

    public void SetLabel(bool shown)
    {
        if (_hoverLabel == null && _hoverLabelText == null) return;
        _hoverLabelText.text = data.objectName;
        _hoverLabelText.gameObject.SetActive(shown);
        _hoverLabel.gameObject.SetActive(shown);
    }
    
    #endregion 
#if UNITY_EDITOR
    [ContextMenu("Create New Line")]
    private void CreateNewLine()
    {
        ConfigureLineRenderer();
        DrawLine();
    }
#endif
}