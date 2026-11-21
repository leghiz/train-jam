using UnityEngine;

public class Furniture : MonoBehaviour
{
    public bool isSurface;

    [Min(0f)] public float surfaceMargin = 0.05f;

    public bool TryGetBounds(out Bounds bounds) => BoundsUtil.TryGetRendererBounds(gameObject, out bounds);

    private void OnDrawGizmosSelected()
    {
        if (!TryGetBounds(out var b)) return;
        Gizmos.color = isSurface ? new Color(0.2f, 1f, 0.4f, 0.8f) : new Color(1f, 1f, 1f, 0.4f);
        Gizmos.DrawWireCube(b.center, b.size);
        if (isSurface)
        {
            var top = new Vector3(b.center.x, b.max.y, b.center.z);
            Gizmos.DrawCube(top, new Vector3(b.size.x - surfaceMargin * 2f, 0.01f, b.size.z - surfaceMargin * 2f));
        }
    }
}

internal static class BoundsUtil
{
    public static bool TryGetRendererBounds(GameObject go, out Bounds bounds)
    {
        bounds = default;
        bool found = false;
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            if (r.GetComponent<TextMesh>()) continue;
            if (!found) { bounds = r.bounds; found = true; }
            else bounds.Encapsulate(r.bounds);
        }
        return found;
    }

    public static Bounds ToLocal(Transform space, Bounds world)
    {
        var min = world.min;
        var max = world.max;
        var local = new Bounds(space.InverseTransformPoint(min), Vector3.zero);
        for (int i = 1; i < 8; i++)
        {
            var corner = new Vector3(
                (i & 1) == 0 ? min.x : max.x,
                (i & 2) == 0 ? min.y : max.y,
                (i & 4) == 0 ? min.z : max.z);
            local.Encapsulate(space.InverseTransformPoint(corner));
        }
        return local;
    }

    public static Rect XZ(Bounds b) => new Rect(b.min.x, b.min.z, b.size.x, b.size.z);

    public static Rect Expand(Rect r, float by) =>
        new Rect(r.xMin - by, r.yMin - by, r.width + by * 2f, r.height + by * 2f);
}
