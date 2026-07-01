using System;
using UnityEngine;

/// <summary>
/// Handles Nina's movement only. Receives a world-space destination and an
/// optional callback that fires when she arrives. No input, no physics.
/// Requires an Animator with an "IsWalking" bool parameter.
///
/// If <see cref="GroundBounds"/> is present, Nina's Y is snapped to the ground
/// surface every frame so she follows ramps and stairs instead of cutting
/// through the air in a straight line.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class NinaController : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [SerializeField, Tooltip("World units per second"), Range(1f, 20f)]
    private float _moveSpeed = 5f;

    [SerializeField, Tooltip("Distance threshold to consider destination reached")]
    private float _arrivalThreshold = 0.05f;

    [SerializeField, Tooltip(
        "How fast Nina's Y tracks the ground surface (world units per second). " +
        "High values = instant snap. Lower values = smooth glide on steep ramps."),
        Range(1f, 50f)]
    private float _groundSnapSpeed = 20f;

    // ── Cached components ────────────────────────────────────────────────────
    private Animator       _animator;
    private SpriteRenderer _spriteRenderer;

    // ── Movement state ───────────────────────────────────────────────────────
    private Vector2 _targetPosition;
    private Action  _onArrival;
    private bool    _isMoving;

    public bool IsMoving => _isMoving;

    // ────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (!TryGetComponent(out _animator))
            Debug.LogError($"[NinaController] Missing Animator on {gameObject.name}", this);

        if (!TryGetComponent(out _spriteRenderer))
            Debug.LogError($"[NinaController] Missing SpriteRenderer on {gameObject.name}", this);
    }

    void Update()
    {
        if (!_isMoving) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            _targetPosition,
            _moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, _targetPosition) < _arrivalThreshold)
        {
            _isMoving = false;
            _animator.SetBool("IsWalking", false);

            Action callback = _onArrival;
            _onArrival = null;
            callback?.Invoke();
        }
    }

    public void MoveTo(Vector2 destination, Action onArrival = null)
    {
        // Clamp destination to valid ground — preserves X, snaps Y only if above surface
        if (GroundBounds.Instance != null)
            destination = GroundBounds.Instance.GetGround(destination.x, destination.y);

        _targetPosition = destination;
        _onArrival      = onArrival;
        _isMoving       = true;

        _animator.SetBool("IsWalking", true);

        if (!Mathf.Approximately(destination.x, transform.position.x))
            _spriteRenderer.flipX = destination.x < transform.position.x;
    }


    /// <summary>Cancels any in-progress movement immediately.</summary>
    public void CancelMovement()
    {
        _isMoving  = false;
        _onArrival = null;
        _animator.SetBool("IsWalking", false);
    }

    private void SnapToGround()
    {
        if (GroundBounds.Instance == null) return;
    
        Vector3 pos    = transform.position;
        float   groundY = GroundBounds.Instance.GetGroundY(pos.x);
        pos.y          = Mathf.MoveTowards(pos.y, groundY, _groundSnapSpeed * Time.deltaTime);
        transform.position = pos;
    }
}