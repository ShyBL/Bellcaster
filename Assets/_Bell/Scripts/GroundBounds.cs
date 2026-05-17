using UnityEngine;

/// <summary>
/// Defines the walkable area for a scene using a <see cref="PolygonCollider2D"/>.
/// Drop this on a dedicated "Ground" GameObject and shape the polygon
/// to cover the entire walkable floor in the scene view.
///
/// Other scripts call <see cref="IsOnGround"/> to validate a click position.
/// The collider is set to Trigger so no physics responses fire.
/// </summary>
[DisallowMultipleComponent]
public class GroundBounds : MonoBehaviour
{
    public static GroundBounds Instance { get; private set; }

    private PolygonCollider2D _poly;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _poly = GetComponent<PolygonCollider2D>();
        _poly.isTrigger = true;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Returns true if <paramref name="worldPos"/> is inside the walkable polygon.</summary>
    public bool IsOnGround(Vector2 worldPos) => _poly.OverlapPoint(worldPos);
}