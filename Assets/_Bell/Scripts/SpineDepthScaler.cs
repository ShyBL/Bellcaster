using UnityEngine;
using Spine.Unity; // Required for Spine integration

/// <summary>
/// Dynamically scales a Spine character based on their Y position relative to GroundBounds.
/// Modifies the Spine Skeleton directly to avoid Unity Transform conflicts.
/// </summary>
[DisallowMultipleComponent]
public class SpineDepthScaler : MonoBehaviour
{
    [Header("Spine Reference")]
    [Tooltip("Drag the GameObject containing the SkeletonAnimation or SkeletonMecanim component here.")]
    [SerializeField] private SkeletonAnimation _spineComponent;

    [Header("Scale Limits")]
    [Tooltip("The scale multiplier when the character is at the very bottom (foreground).")]
    [SerializeField] private float _maxScale = 1.0f;
    
    [Tooltip("The scale multiplier when the character is at the very top (background).")]
    [SerializeField] private float _minScale = 0.5f;

    [Header("Custom Y Boundaries (Optional)")]
    [Tooltip("If true, ignores GroundBounds' shape and uses custom Y values below.")]
    [SerializeField] private bool _useCustomYBounds = false;
    [SerializeField] private float _customMinY = -5f;
    [SerializeField] private float _customMaxY = 5f;

    private float _initialScaleX = 1f;
    private float _initialScaleY = 1f;

    void Start()
    {
        if (_spineComponent == null)
        {
            _spineComponent = GetComponentInChildren<SkeletonAnimation>();
        }

        if (_spineComponent != null && _spineComponent.Skeleton != null)
        {
            // Store the initial scale dictated by your Skeleton/Spine setup
            _initialScaleX = Mathf.Abs(_spineComponent.Skeleton.ScaleX);
            _initialScaleY = Mathf.Abs(_spineComponent.Skeleton.ScaleY);
        }
    }

    void LateUpdate()
    {
        if (NavigationManager.Instance == null || NavigationManager.Instance.CurrentGround == null) return;
        if (_spineComponent == null || _spineComponent.Skeleton == null) return;

        GroundBounds activeGround = NavigationManager.Instance.CurrentGround;

        // 1. Get the vertical boundaries
        float minY = _useCustomYBounds ? _customMinY : activeGround.MinWalkableY;
        float maxY = _useCustomYBounds ? _customMaxY : activeGround.MaxWalkableY;
        
        // if (GroundBounds.Instance == null || _spineComponent == null || _spineComponent.Skeleton == null) return;
        //
        // // 1. Get the vertical boundaries
        // float minY = _useCustomYBounds ? _customMinY : GroundBounds.Instance.MinWalkableY;
        // float maxY = _useCustomYBounds ? _customMaxY : GroundBounds.Instance.MaxWalkableY;

        if (Mathf.Approximately(minY, maxY)) return;

        // 2. Clamp current Y within limits and normalize it to a 0 to 1 range
        float currentY = Mathf.Clamp(transform.position.y, minY, maxY);
        float t = Mathf.InverseLerp(minY, maxY, currentY);

        // 3. Interpolate scale
        float targetScaleMultiplier = Mathf.Lerp(_maxScale, _minScale, t);

        // 4. Apply directly to Spine's internal skeleton
        // We read the current sign of ScaleX to ensure we don't break your left/right flipping logic!
        float currentXSign = Mathf.Sign(_spineComponent.Skeleton.ScaleX);

        _spineComponent.Skeleton.ScaleX = _initialScaleX * targetScaleMultiplier * currentXSign;
        _spineComponent.Skeleton.ScaleY = _initialScaleY * targetScaleMultiplier;
    }
}