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
    /// <summary>
    /// Tracks whether a wrong item has already been tried on this object once.
    /// First failure shows data.wrongItemText/wrongItemVO; repeats fall back
    /// to NinaQuipBank's generic lines instead of repeating the specific one.
    /// </summary>
    private bool _hasFailedAttemptBefore = false;
    #region Helpers
    
    private InteractionType? GetActiveInteraction()
    {
        if (data.canExamine)
            return InteractionType.Examine;

        if (data.canPickUp && !hasBeenInteracted)
        {
            if (WorldState.Instance.CheckRequirement(data.pickupRequirement))
                return InteractionType.PickUp;
        }

        if (data.canInteract && !hasBeenInteracted)
            return InteractionType.Interact;

        if (data.canNavigate && !hasBeenInteracted)
            return InteractionType.Navigate;

        return null;
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
    
    /// <summary>
    /// Called when an inventory item is dropped on this object via drag-and-drop (from
    /// either the Pack UI or a Toolbelt world item). Returns true if accepted (runs the
    /// same effects OnInteract would from a click), false if this isn't a valid use of
    /// that item here — in which case a wrong-item quip is shown before returning false.
    /// </summary>
    public bool TryUseItem(InteractableData item)
    {
        if (hasBeenInteracted) return false;
        if (!data.canInteract) return false;
        if (item == null) return false;

        bool isCorrectItem = !string.IsNullOrEmpty(data.requiredInventoryItem)
                             && item.objectName == data.requiredInventoryItem;

        if (!isCorrectItem)
        {
            ShowWrongItemResponse();
            return false;
        }

        OnInteract();
        return true;
    }
    
    private void ShowWrongItemResponse()
    {
        if (NinaSpeechBubble.Instance == null) return;

        if (!_hasFailedAttemptBefore && !string.IsNullOrEmpty(data.wrongItemText))
        {
            _hasFailedAttemptBefore = true;
            NinaSpeechBubble.Instance.Show(data.wrongItemText, data.wrongItemVO);
            return;
        }

        _hasFailedAttemptBefore = true;
        NinaSpeechBubble.Instance.ShowCategory(NinaQuipCategory.WrongItemFallback);
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

        InteractionType? active = GetActiveInteraction();
        if (active == null) return;

        switch (active.Value)
        {
            case InteractionType.Examine:  OnExamine();  break;
            case InteractionType.PickUp:   OnPickUp();   break;
            case InteractionType.Interact: OnInteract(); break;
            case InteractionType.Navigate: OnNavigate(); break;
        }
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
        
        switch (data.pickupDestination)
        {
            case PickupDestination.Inventory:
                InventoryManager.Instance.AddToPack(data);
                break;
            case PickupDestination.Souvenir:
                InventoryManager.Instance.AddSouvenir(data);
                break;
            case PickupDestination.Journal:
                InventoryManager.Instance.AddJournal(data);
                break;
            case PickupDestination.Toolbelt:
                InventoryManager.Instance.Equip(data);
                break;
        }
        
        hasBeenInteracted = true;
        gameObject.SetActive(false); // Remove from scene
    }
    
    public void OnInteract()
    {
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
           if (PlayerInputHandler.Instance != null)
               PlayerInputHandler.Instance.RefreshInteractables();
        }
        
        hasBeenInteracted = true;
    }
    
    public void OnNavigate()
    {
        if (NavigationManager.Instance != null)
            NavigationManager.Instance.NavigateTo(data.targetAreaName);
        else
            Debug.LogError("[Interactable] NavigationManager instance not found!");

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