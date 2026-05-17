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