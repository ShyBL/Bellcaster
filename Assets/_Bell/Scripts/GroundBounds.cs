using UnityEngine;

/// <summary>
/// Defines the walkable area for a scene using a <see cref="PolygonCollider2D"/>.
/// Drop this on a dedicated "Ground" GameObject and shape the polygon
/// to cover the entire walkable floor in the scene view.
///
/// Other scripts call <see cref="IsOnGround"/> to validate a click position.
/// The collider is set to Trigger so no physics responses fire.
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
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

    /// <summary>
    /// Returns a valid ground position for the given destination.
    /// - If requestedY is on or below the polygon's top surface at x, returns (x, requestedY) unchanged.
    /// - If requestedY is above the top surface (click was too high), clamps Y down to the surface.
    /// Falls back to (x, transform.position.y) if no polygon edge crosses x.
    /// </summary>
    public Vector2 GetGround(float x, float y)
    {
        float topY = GetTopSurfaceY(x);
        return new Vector2(x, Mathf.Min(y, topY));
    }
    
    /// <summary>
    /// Returns the Y of the polygon's highest (top) edge at worldX.
    /// This is what NinaController uses to snap per-frame Y while walking.
    /// </summary>
    public float GetTopSurfaceY(float worldX)
    {
        Vector2[] pts  = _poly.points;
        float     best = transform.position.y;
        bool      hit  = false;

        for (int i = 0; i < pts.Length; i++)
        {
            Vector2 a = transform.TransformPoint(pts[i]);
            Vector2 b = transform.TransformPoint(pts[(i + 1) % pts.Length]);

            if (Mathf.Approximately(a.x, b.x)) continue;
            if (worldX < Mathf.Min(a.x, b.x) || worldX > Mathf.Max(a.x, b.x)) continue;

            float t = Mathf.InverseLerp(a.x, b.x, worldX);
            float y = Mathf.Lerp(a.y, b.y, t);

            if (!hit || y > best) { best = y; hit = true; }
        }

        return best;
    }
    
    /// <summary>
    /// Returns the closest point on the polygon's perimeter to <paramref name="worldPos"/>.
    /// Used when a click lands outside the walkable area.
    /// </summary>
    public Vector2 ClosestPointOnBoundary(Vector2 worldPos)
    {
        Vector2[] pts       = _poly.points;
        Vector2   best      = Vector2.zero;
        float     bestDistSq = float.MaxValue;

        for (int i = 0; i < pts.Length; i++)
        {
            Vector2 a = transform.TransformPoint(pts[i]);
            Vector2 b = transform.TransformPoint(pts[(i + 1) % pts.Length]);

            Vector2 closest = ClosestPointOnSegment(a, b, worldPos);
            float   distSq  = (closest - worldPos).sqrMagnitude;

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best       = closest;
            }
        }

        return best;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        Vector2 ab = b - a;
        float   t  = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        return a + Mathf.Clamp01(t) * ab;
    }
    
    /// <summary>
    /// Returns the Y coordinate of the ground surface at <paramref name="worldX"/>.
    /// Walks every polygon edge, finds those that cross worldX, and returns the
    /// highest Y intersection — ignoring the bottom face of the polygon.
    /// Falls back to this transform's Y if no edge crosses worldX.
    /// </summary>
    public float GetGroundY(float worldX)
    {
        Vector2[] pts  = _poly.points;
        
        float     best = transform.position.y;
        bool      hit  = false;

        for (int i = 0; i < pts.Length; i++)
        {
            Vector2 a = transform.TransformPoint(pts[i]);
            Vector2 b = transform.TransformPoint(pts[(i + 1) % pts.Length]);

            // Skip edges that don't span worldX
            if (Mathf.Approximately(a.x, b.x)) continue;
            if (worldX < Mathf.Min(a.x, b.x) || worldX > Mathf.Max(a.x, b.x)) continue;

            float t = Mathf.InverseLerp(a.x, b.x, worldX);
            float y = Mathf.Lerp(a.y, b.y, t);

            if (!hit || y > best) { best = y; hit = true; }
        }

        return best;
    }
}