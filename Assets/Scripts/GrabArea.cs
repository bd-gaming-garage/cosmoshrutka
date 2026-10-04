using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class GrabArea : MonoBehaviour
{
    [Tooltip("Outline in local coordinates, ordered around the perimeter")]
    [SerializeField] private Vector2[] points =
    {
        new(-40f, 30f),
        new(40f, 30f),
        new(40f, -100f),
        new(-40f, -100f)
    };

    private readonly Vector3[] _worldCorners = new Vector3[4];
    private readonly Vector2[] _localCorners = new Vector2[4];

    public bool Overlaps(RectTransform target)
    {
        if (!isActiveAndEnabled || target == null ||
            points == null || points.Length < 3)
        {
            return false;
        }

        target.GetWorldCorners(_worldCorners);
        for (int i = 0; i < 4; i++)
        {
            _localCorners[i] = transform.InverseTransformPoint(_worldCorners[i]);
        }

        // A corner of the item lies inside the hand.
        foreach (Vector2 corner in _localCorners)
        {
            if (Contains(points, corner)) return true;
        }

        // The item contains part or all of the hand.
        foreach (Vector2 point in points)
        {
            if (Contains(_localCorners, point)) return true;
        }

        // Edges can cross without either shape containing a vertex.
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Length];

            for (int j = 0; j < 4; j++)
            {
                if (SegmentsIntersect(
                        a, b, _localCorners[j], _localCorners[(j + 1) % 4]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool Contains(Vector2[] polygon, Vector2 point)
    {
        bool inside = false;

        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[j];
            Vector2 b = polygon[i];

            if (IsOnSegment(a, b, point)) return true;

            if ((a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static bool IsOnSegment(Vector2 a, Vector2 b, Vector2 point)
    {
        const float tolerance = 0.001f;
        Vector2 edge = b - a;

        if (edge.sqrMagnitude < tolerance * tolerance)
            return (point - a).sqrMagnitude <= tolerance * tolerance;

        float t = Vector2.Dot(point - a, edge) / edge.sqrMagnitude;
        Vector2 closest = a + Mathf.Clamp01(t) * edge;
        return (point - closest).sqrMagnitude <= tolerance * tolerance;
    }

    private static bool SegmentsIntersect(
        Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (IsOnSegment(a, b, c) || IsOnSegment(a, b, d) ||
            IsOnSegment(c, d, a) || IsOnSegment(c, d, b))
        {
            return true;
        }

        float abC = Cross(b - a, c - a);
        float abD = Cross(b - a, d - a);
        float cdA = Cross(d - c, a - c);
        float cdB = Cross(d - c, b - c);

        return ((abC > 0f && abD < 0f) || (abC < 0f && abD > 0f)) &&
               ((cdA > 0f && cdB < 0f) || (cdA < 0f && cdB > 0f));
    }

    private void OnDrawGizmosSelected()
    {
        if (points == null || points.Length < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(points[i]),
                transform.TransformPoint(points[(i + 1) % points.Length]));
        }
    }
}
