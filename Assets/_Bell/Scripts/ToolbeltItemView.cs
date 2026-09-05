using UnityEngine;

/// <summary>
/// The world-space visual for a single equipped Toolbelt item — a single
/// shared prefab, bound with whatever InteractableData is currently
/// equipped at that socket, same "shared view, bound with data" shape as
/// PackSlotView/InventorySlotUI. Spawned by ToolbeltAttachmentPoint.Attach().
///
/// A plain click shows the item's examine text (reuses ExamineTextDisplay,
/// same as clicking a Souvenir in the Journal). Dragging is driven entirely
/// by PlayerInputHandler's world-space drag system — this component doesn't
/// implement any input handling itself, it's purely a data-bound visual.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class ToolbeltItemView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _collider;

    public InteractableData Data { get; private set; }
    public BoxCollider2D Collider => _collider;

    private void Reset()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
    }

    public void Bind(InteractableData data)
    {
        Data = data;

        if (_spriteRenderer != null)
            _spriteRenderer.sprite = data.objectIcon;

        // Auto-size the collider to whichever icon this particular item
        // uses, since a Bell and a Hammer won't share the same dimensions.
        if (_collider != null && _spriteRenderer != null && _spriteRenderer.sprite != null)
        {
            _collider.size = _spriteRenderer.sprite.bounds.size;
            _collider.offset = _spriteRenderer.sprite.bounds.center;
        }
    }

    /// <summary>Called by PlayerInputHandler on a plain click (no drag).</summary>
    public void OnClicked()
    {
        if (Data == null) return;

        if (ExamineTextDisplay.Instance != null)
            ExamineTextDisplay.Instance.ShowText(Data.examineText);

        if (Data.examineVO != null)
            AudioSource.PlayClipAtPoint(Data.examineVO, transform.position);
    }
}
