using System;
using FMODUnity;
using UnityEngine;
using Spine.Unity;

/// <summary>
/// Handles Nina's movement only. Receives a world-space destination and an
/// optional callback that fires when she arrives. No input, no physics.
/// Requires an Animator with an "IsWalking" bool parameter.
///
/// If <see cref="GroundBounds"/> is present, Nina's Y is snapped to the ground
/// surface every frame so she follows ramps and stairs instead of cutting
/// through the air in a straight line.
/// </summary>

public class NinaController : MonoBehaviour
{
    [SerializeField, Tooltip("World units per second"), Range(1f,
         20f)]
    private float _moveSpeed = 5f;

    [SerializeField, Tooltip("Distance threshold to consider destination reached")]
    private float _arrivalThreshold = 0.05f;

    [SerializeField, Tooltip(
         "How fast Nina's Y tracks the ground surface (world units per second). " +
         "High values = instant snap. Lower values = smooth glide on steep ramps."),
     Range(1f,
         50f)]
    private float _groundSnapSpeed = 20f;
    
    [Header("Audio")]
    [SerializeField, Tooltip("FMOD Event for Footsteps")]
    private EventReference _footstepEvent;
    [SerializeField, Tooltip("How far Nina moves before the next footstep plays")]
    private float _stepDistance = 1.5f;
    
    // ── Cached components ────────────────────────────────────────────────────
    private SkeletonAnimation _skeletonAnimation;

    // ── Movement state ───────────────────────────────────────────────────────
    private Vector2 _targetPosition;
    private Action _onArrival;
    private bool _isMoving;
    private Vector2 _lastStepPosition;
    public bool IsMoving =>
        _isMoving;

    // ────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (!TryGetComponent(out _skeletonAnimation))
            Debug.LogError($"[NinaController] Missing SkeletonAnimation on {gameObject.name}",
                this);
    }

    void Update()
    {
        if (!_isMoving) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            _targetPosition,
            _moveSpeed * Time.deltaTime
        );

        // Distance-based footsteps while walking
        if (Vector2.Distance(transform.position, _lastStepPosition) >= _stepDistance)
        {
            RuntimeManager.PlayOneShot(_footstepEvent, transform.position);
            _lastStepPosition = transform.position;
        }

        // Arrival logic
        if (Vector2.Distance(transform.position, _targetPosition) < _arrivalThreshold)
        {
            _isMoving = false;
            
            // 2. PLAY SOUND ON ARRIVAL: The final "plant foot" step
            RuntimeManager.PlayOneShot(_footstepEvent, transform.position);
            
            _skeletonAnimation.AnimationState.SetAnimation(0, "adle", true); 

            Action callback = _onArrival;
            _onArrival = null;
            callback?.Invoke();
        }
    }

    public void MoveTo(Vector2 destination,
        Action onArrival = null)
    {
        // Clamp destination to valid ground — preserves X, snaps Y only if above surface
        if (GroundBounds.Instance != null)
            destination = GroundBounds.Instance.GetGround(destination.x,
                destination.y);

        _targetPosition = destination;
        _onArrival = onArrival;
        _isMoving = true;

        // 1. PLAY SOUND ON START: The initial "push off" step
        RuntimeManager.PlayOneShot(_footstepEvent, transform.position);
        
        // Reset the tracker so the next distance-based step measures from here
        _lastStepPosition = transform.position;
        
        // Track 0, "walk" (or "run"), loop (true)
        _skeletonAnimation.AnimationState.SetAnimation(0,
            "walk",
            true);

        if (!Mathf.Approximately(destination.x,
                transform.position.x))
        {
            // Spine uses 1 for normal facing, -1 for flipped facing
            float facingDirection = (destination.x < transform.position.x)
                ? 1f
                : -1f;
            _skeletonAnimation.skeleton.ScaleX = facingDirection;
        }
    }


    /// <summary>Cancels any in-progress movement immediately.</summary>
    public void CancelMovement()
    {
        _isMoving = false;
        _onArrival = null;
        _skeletonAnimation.AnimationState.SetAnimation(0,
            "adle",
            true);
    }

    // private void SnapToGround()
    // {
    //     if (GroundBounds.Instance == null) return;
    //
    //     Vector3 pos    = transform.position;
    //     float   groundY = GroundBounds.Instance.GetGroundY(pos.x);
    //     pos.y          = Mathf.MoveTowards(pos.y, groundY, _groundSnapSpeed * Time.deltaTime);
    //     transform.position = pos;
    // }
}