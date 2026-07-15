using System.Collections.Generic;
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

    [Header("Pathfinding Settings")]
    [Tooltip("How far the path waypoints are offset from obstacle corners so the character doesn't clip them.")]
    [SerializeField] private float _characterOffset = 0.35f;
    public float MinWalkableY => _poly.bounds.min.y;
    public float MaxWalkableY => _poly.bounds.max.y;
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
    
    public Vector2 ClosestPointOnBoundary(Vector2 worldPos)
    {
        Vector2   best      = Vector2.zero;
        float     bestDistSq = float.MaxValue;
        List<Edge> edges     = GetWorldEdges();

        foreach (var edge in edges)
        {
            Vector2 closest = ClosestPointOnSegment(edge.A, edge.B, worldPos);
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

    #region Pathfinding System

    private struct Edge
    {
        public Vector2 A;
        public Vector2 B;
    }

    /// <summary>
    /// Finds a safe path of waypoints around obstacles from start to end.
    /// </summary>
    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        if (!IsOnGround(start)) start = ClosestPointOnBoundary(start);
        if (!IsOnGround(end)) end = ClosestPointOnBoundary(end);

        List<Edge> edges = GetWorldEdges();

        // If there's direct line of sight, just walk straight!
        if (HasLineOfSight(start, end, edges))
        {
            return new List<Vector2> { start, end };
        }

        // Generate our pathfinding graph nodes (Start, End, and Offset Polygon Vertices)
        List<Vector2> nodes = GetPathfindingNodes(start, end);
        int n = nodes.Count;

        // Build adjacency visibility list
        List<int>[] adj = new List<int>[n];
        for (int i = 0; i < n; i++) adj[i] = new List<int>();

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (HasLineOfSight(nodes[i], nodes[j], edges))
                {
                    adj[i].Add(j);
                    adj[j].Add(i);
                }
            }
        }

        // A* Pathfinding Solver
        float[] gScore = new float[n];
        float[] fScore = new float[n];
        int[] cameFrom = new int[n];
        for (int i = 0; i < n; i++)
        {
            gScore[i] = float.MaxValue;
            fScore[i] = float.MaxValue;
            cameFrom[i] = -1;
        }

        gScore[0] = 0; // Index 0 is start
        fScore[0] = Vector2.Distance(start, end);

        List<int> openSet = new List<int> { 0 };

        while (openSet.Count > 0)
        {
            int current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (fScore[openSet[i]] < fScore[current])
                {
                    current = openSet[i];
                }
            }

            if (current == 1) // Index 1 is end
            {
                List<Vector2> path = new List<Vector2>();
                int currIdx = current;
                while (currIdx != -1)
                {
                    path.Add(nodes[currIdx]);
                    currIdx = cameFrom[currIdx];
                }
                path.Reverse();
                return path;
            }

            openSet.Remove(current);

            foreach (int neighbor in adj[current])
            {
                float tentativeG = gScore[current] + Vector2.Distance(nodes[current], nodes[neighbor]);
                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Vector2.Distance(nodes[neighbor], end);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<Vector2> { start, end }; // Fallback to linear if path fails
    }

    private bool HasLineOfSight(Vector2 a, Vector2 b, List<Edge> edges)
    {
        foreach (var edge in edges)
        {
            if (LineSegmentsIntersect(a, b, edge.A, edge.B))
            {
                return false;
            }
        }
        // Double-check midpoint to make sure the line doesn't bypass external areas
        Vector2 midpoint = (a + b) * 0.5f;
        return IsOnGround(midpoint);
    }

    private List<Edge> GetWorldEdges()
    {
        List<Edge> edges = new List<Edge>();
        for (int p = 0; p < _poly.pathCount; p++)
        {
            Vector2[] pathPts = _poly.GetPath(p);
            for (int i = 0; i < pathPts.Length; i++)
            {
                Vector2 a = transform.TransformPoint(pathPts[i]);
                Vector2 b = transform.TransformPoint(pathPts[(i + 1) % pathPts.Length]);
                edges.Add(new Edge { A = a, B = b });
            }
        }
        return edges;
    }

    private List<Vector2> GetPathfindingNodes(Vector2 start, Vector2 end)
    {
        List<Vector2> nodes = new List<Vector2> { start, end };

        for (int p = 0; p < _poly.pathCount; p++)
        {
            Vector2[] pathPts = _poly.GetPath(p);
            int count = pathPts.Length;
            for (int i = 0; i < count; i++)
            {
                Vector2 prev = transform.TransformPoint(pathPts[(i - 1 + count) % count]);
                Vector2 curr = transform.TransformPoint(pathPts[i]);
                Vector2 next = transform.TransformPoint(pathPts[(i + 1) % count]);

                nodes.Add(GetOffsetVertex(prev, curr, next, _characterOffset));
            }
        }
        return nodes;
    }

    private Vector2 GetOffsetVertex(Vector2 prev, Vector2 curr, Vector2 next, float offsetDist)
    {
        Vector2 dir1 = (curr - prev).normalized;
        Vector2 dir2 = (next - curr).normalized;
        Vector2 normal = new Vector2(-dir1.y, dir1.x); 
        
        Vector2 bisector = (dir1 - dir2).normalized;
        if (bisector.sqrMagnitude < 0.001f) bisector = normal;
        
        // Push the corner point slightly inside the walkable zone
        Vector2 p1 = curr + bisector * offsetDist;
        Vector2 p2 = curr - bisector * offsetDist;
        
        if (IsOnGround(p1)) return p1;
        if (IsOnGround(p2)) return p2;
        return curr;
    }

    private static bool LineSegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
        if (Mathf.Approximately(d, 0)) return false; // Parallel

        float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

        const float eps = 0.01f; // Epsilon handles start/end vertex touch offsets
        return (u > eps && u < 1f - eps && v > eps && v < 1f - eps);
    }

    #endregion
}