using UnityEngine;

/// <summary>
/// Defines the walkable ground for a scene using an <see cref="EdgeCollider2D"/>.
/// Drop this on a dedicated "Ground" GameObject and shape the edge collider
/// to match the walkable floor in the scene view.
///
/// Other scripts call <see cref="ClampToGround"/> to snap a world position
/// to the nearest valid point on the path.
///
/// The collider is set to Trigger so no physics responses fire; it is used
/// purely for spatial queries.
/// </summary>
[RequireComponent(typeof(EdgeCollider2D))]
[DisallowMultipleComponent]
public class GroundBounds : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
    public static GroundBounds Instance { get; private set; }

    // ── Cached ───────────────────────────────────────────────────────────────
    private EdgeCollider2D _edge;

    // ────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("[GroundBounds] Multiple instances found. Destroying duplicate.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!TryGetComponent(out _edge))
        {
            Debug.LogError("[GroundBounds] Missing EdgeCollider2D.", this);
            return;
        }

        // Trigger-only: detect position but don't push the character.
        _edge.isTrigger = true;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the point on the ground path closest to <paramref name="worldPos"/>.
    /// Use this to clamp a click destination or cursor position to the floor.
    /// </summary>
    public Vector2 ClampToGround(Vector2 worldPos)
    {
        if (_edge == null) return worldPos;

        Vector2[] pts = _edge.points;
        if (pts.Length < 2) return worldPos;

        Vector2 best     = Vector2.zero;
        float   bestDist = float.MaxValue;

        // Walk each segment and find the closest point on any segment
        for (int i = 0; i < pts.Length - 1; i++)
        {
            // EdgeCollider2D points are in local space
            Vector2 a = transform.TransformPoint(pts[i]);
            Vector2 b = transform.TransformPoint(pts[i + 1]);

            Vector2 candidate = ClosestPointOnSegment(worldPos, a, b);
            float   dist      = Vector2.SqrMagnitude(worldPos - candidate);

            if (dist < bestDist)
            {
                bestDist = dist;
                best     = candidate;
            }
        }

        return best;
    }

    /// <summary>
    /// Returns true when <paramref name="worldPos"/> is within
    /// <paramref name="tolerance"/> world units of the ground path.
    /// </summary>
    public bool IsOnGround(Vector2 worldPos, float tolerance = 0.5f)
    {
        return Vector2.Distance(worldPos, ClampToGround(worldPos)) <= tolerance;
    }

    // ────────────────────────────────────────────────────────────────────────
    private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float   t  = Vector2.Dot(point - a, ab) / Vector2.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }
}
