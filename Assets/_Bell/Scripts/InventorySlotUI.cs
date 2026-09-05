using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Shared slot prefab used by both PackPanelController and JournalPanelController.
/// Hover shows a highlight + name label — the same visual language as the world
/// Interactable's SetHighlight/SetLabel, just re-implemented for uGUI via
/// IPointerEnter/IPointerExit. Journal slots can additionally be "locked": the real
/// icon/name are hidden behind a silhouette + "???" overlay and clicks are ignored.
///
/// Pack slots are also draggable: dragging the icon onto a world Interactable and
/// releasing calls PlayerInputHandler.TryUseItemAtScreenPoint, which walks Nina over
/// and asks the Interactable whether it accepts this item. Journal slots are passed
/// draggable: false and never enter the drag flow.
/// </summary>
[DisallowMultipleComponent]
public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Base")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameLabel;
    [SerializeField] private GameObject _highlight;

    [Header("Locked State (Journal only — leave unlocked slots always false)")]
    [SerializeField] private GameObject _lockedOverlay;

    [Header("Drag (Pack only)")]
    [SerializeField, Tooltip("Root Canvas to parent the drag ghost under. Auto-found if left empty.")]
    private Canvas _rootCanvas;
    [SerializeField, Range(0f, 1f), Tooltip("Alpha applied to this slot's icon while it's being dragged.")]
    private float _draggingIconAlpha = 0.35f;

    private InteractableData _data;
    private bool _locked;
    private bool _draggable;

    private RectTransform _dragGhost;
    private Interactable _dragHoverTarget;
    private Color _iconRestoreColor = Color.white;

    /// <summary>Fired on click. Never fires while the slot is locked.</summary>
    public event Action<InteractableData> OnSlotClicked;

    void Awake()
    {
        SetHighlight(false);
        SetLabel(false);

        if (_rootCanvas == null)
            _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
    }

    /// <summary>Populates this slot. Pass locked: true for undiscovered Journal entries,
    /// and draggable: false for any slot that shouldn't support drag-to-use (Journal).</summary>
    public void Setup(InteractableData data, bool locked = false, bool draggable = true)
    {
        _data = data;
        _locked = locked;
        _draggable = draggable && !locked;

        if (_icon != null)
        {
            _icon.sprite = data.objectIcon;
            _icon.enabled = !locked;
            _icon.color = Color.white;
        }

        if (_nameLabel != null)
            _nameLabel.text = data.objectName;

        if (_lockedOverlay != null)
            _lockedOverlay.SetActive(locked);
    }

    public void SetHighlight(bool shown)
    {
        if (_highlight != null)
            _highlight.SetActive(shown);
    }

    public void SetLabel(bool shown)
    {
        if (_nameLabel != null)
            _nameLabel.gameObject.SetActive(shown && !_locked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHighlight(true);
        SetLabel(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
        SetLabel(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_locked || _data == null) return;
        OnSlotClicked?.Invoke(_data);
    }

    #region Drag-to-use

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_draggable || _data == null || _icon == null || _icon.sprite == null) return;

        _iconRestoreColor = _icon.color;
        _icon.color = new Color(_iconRestoreColor.r, _iconRestoreColor.g, _iconRestoreColor.b, _draggingIconAlpha);

        GameObject ghostObj = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        Transform ghostParent = _rootCanvas != null ? _rootCanvas.transform : transform.root;
        ghostObj.transform.SetParent(ghostParent, false);
        ghostObj.transform.SetAsLastSibling();

        Image ghostImage = ghostObj.GetComponent<Image>();
        ghostImage.sprite = _icon.sprite;
        ghostImage.raycastTarget = false;
        ghostObj.GetComponent<CanvasGroup>().blocksRaycasts = false;

        _dragGhost = (RectTransform)ghostObj.transform;
        _dragGhost.sizeDelta = _icon.rectTransform.sizeDelta;
        _dragGhost.position = eventData.position; // Overlay-canvas assumption: screen px == world pos
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragGhost != null)
            _dragGhost.position = eventData.position;

        UpdateDragHover(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragGhost != null)
        {
            Destroy(_dragGhost.gameObject);
            _dragGhost = null;
        }

        if (_dragHoverTarget != null)
        {
            _dragHoverTarget.SetHighlight(false);
            _dragHoverTarget = null;
        }

        if (_icon != null)
            _icon.color = _iconRestoreColor;

        if (!_draggable || _data == null) return;
        if (eventData.pointerCurrentRaycast.gameObject != null) return; // dropped back onto UI, not the world

        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.TryUseItemAtScreenPoint(_data, eventData.position);
    }

    private void UpdateDragHover(PointerEventData eventData)
    {
        Interactable target = null;

        if (eventData.pointerCurrentRaycast.gameObject == null && PlayerInputHandler.Instance != null)
            target = PlayerInputHandler.Instance.GetInteractableAtScreenPoint(eventData.position);

        if (target == _dragHoverTarget) return;

        if (_dragHoverTarget != null)
            _dragHoverTarget.SetHighlight(false);

        _dragHoverTarget = target;

        if (_dragHoverTarget != null)
            _dragHoverTarget.SetHighlight(true);
    }

    #endregion
}
