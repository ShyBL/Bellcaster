using UnityEngine;

/// <summary>
/// Sits alongside <see cref="Interactable"/> on each interactable GameObject.
/// Owns the LineRenderer outline (drawn from a PolygonCollider2D) and the
/// interaction-point transform Nina walks toward.
///
/// The outline is off by default; <see cref="PlayerInputHandler"/> toggles it
/// as the cursor hovers or cycles to this object.
///
/// When Nina arrives, this script calls <see cref="InteractionMenu.ShowMenu"/>.
/// </summary>
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(LineRenderer))]
[DisallowMultipleComponent]
public class InteractableView : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [SerializeField, Tooltip("Where Nina walks to before the menu opens. " +
        "Leave empty to use this transform's position.")]
    private Transform _interactionPoint;

    [SerializeField, Tooltip("Outline width in world units")]
    private float _outlineWidth = 0.05f;

    [SerializeField]
    private Color _outlineColor = Color.yellow;

    // ── Cached components ────────────────────────────────────────────────────
    private Interactable     _interactable;
    private PolygonCollider2D _polygonCollider;
    private LineRenderer     _lineRenderer;

    // ────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (!TryGetComponent(out _interactable))
            Debug.LogError($"[InteractableView] Missing Interactable on {gameObject.name}", this);

        if (!TryGetComponent(out _polygonCollider))
            Debug.LogError($"[InteractableView] Missing PolygonCollider2D on {gameObject.name}", this);

        if (!TryGetComponent(out _lineRenderer))
            Debug.LogError($"[InteractableView] Missing LineRenderer on {gameObject.name}", this);

        ConfigureLineRenderer();
        DrawLine();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>World position Nina should walk toward.</summary>
    public Vector2 InteractionPosition =>
        _interactionPoint != null
            ? (Vector2)_interactionPoint.position
            : (Vector2)transform.position;

    /// <summary>The data layer component on this GameObject.</summary>
    public Interactable Interactable => _interactable;

    /// <summary>Shows or hides the outline highlight.</summary>
    public void SetHighlight(bool highlighted)
    {
        if (_lineRenderer != null)
            _lineRenderer.enabled = highlighted;
    }

    /// <summary>
    /// Called by <see cref="PlayerInputHandler"/> after Nina arrives.
    /// Opens the interaction menu for this object.
    /// </summary>
    public void OpenMenu()
    {
        if (_interactable == null || InteractionMenu.Instance == null) return;

        var available = _interactable.GetAvailableInteractions();
        InteractionMenu.Instance.ShowMenu(_interactable, transform.position, available);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Outline drawing (matches the DrawLine signature in the brief)
    // ────────────────────────────────────────────────────────────────────────
    private void ConfigureLineRenderer()
    {
        if (_lineRenderer == null) return;

        _lineRenderer.useWorldSpace    = false; // points are in local space
        _lineRenderer.loop             = true;
        _lineRenderer.startColor       = _outlineColor;
        _lineRenderer.endColor         = _outlineColor;
        _lineRenderer.startWidth       = _outlineWidth;
        _lineRenderer.endWidth         = _outlineWidth;
        _lineRenderer.sortingLayerName = "UI";  // adjust to your sorting layer
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
}
