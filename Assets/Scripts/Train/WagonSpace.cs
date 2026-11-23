using System.Collections.Generic;
using UnityEngine;

internal class WagonSpace
{
    private const int Attempts = 30;
    private const float PlayerSpawnClearance = 0.8f;
    private const float RaisedThreshold = 0.05f;
    private const float SupportRayHeight = 1f;
    private const float SupportTolerance = 0.1f;
    private const float SupportEdgeMargin = 0.06f;

    private class Surface
    {
        public Furniture furniture;
        public Rect area;
        public float top;
        public readonly List<Rect> taken = new List<Rect>();
    }

    private readonly Transform _wagon;
    private readonly Rect _floor;
    private readonly float _floorY;
    private readonly float _spacing;
    private readonly List<Rect> _floorTaken = new List<Rect>();
    private readonly List<Surface> _surfaces = new List<Surface>();

    public bool HasSurfaces => _surfaces.Count > 0;

    public WagonSpace(WagonLayout wagon, float spacing)
    {
        Physics.SyncTransforms();

        _wagon = wagon.transform;
        _floor = wagon.GetFloorRect();
        _floorY = wagon.GetFloorHeight();
        _spacing = spacing;

        float door = wagon.doorClearance;
        _floorTaken.Add(new Rect(_floor.xMin, _floor.yMin, door, _floor.height));
        _floorTaken.Add(new Rect(_floor.xMax - door, _floor.yMin, door, _floor.height));

        if (wagon.playerSpawn)
        {
            var p = _wagon.InverseTransformPoint(wagon.playerSpawn.position);
            _floorTaken.Add(new Rect(p.x - PlayerSpawnClearance, p.z - PlayerSpawnClearance,
                                     PlayerSpawnClearance * 2f, PlayerSpawnClearance * 2f));
        }

        var lyingOnTop = new List<Rect>();
        foreach (var furniture in wagon.GetFurniture())
        {
            if (!furniture.TryGetBounds(out var world)) continue;
            var local = BoundsUtil.ToLocal(_wagon, world);
            var footprint = BoundsUtil.XZ(local);

            bool raised = local.min.y > _floorY + RaisedThreshold;
            if (raised && !furniture.isSurface)
            {
                lyingOnTop.Add(footprint);
                continue;
            }

            _floorTaken.Add(footprint);

            if (!furniture.isSurface) continue;
            var area = BoundsUtil.Expand(footprint, -furniture.surfaceMargin);
            if (area.width > 0f && area.height > 0f)
            {
                _surfaces.Add(new Surface { furniture = furniture, area = area, top = local.max.y });
            }
        }

        foreach (var rect in lyingOnTop)
        {
            foreach (var surface in _surfaces)
            {
                if (surface.area.Overlaps(rect)) surface.taken.Add(rect);
            }
        }
    }

    public bool TryPlaceOnFloor(Transform obj, System.Random rng, System.Func<Vector3, bool> accept = null)
    {
        var shape = Measure(obj);
        int attempts = accept == null ? Attempts : Attempts * 2;
        for (int i = 0; i < attempts; i++)
        {
            if (!TryRandomPivot(_floor, shape, rng, out var pivot, out var rect)) return false;
            if (Overlaps(rect, _floorTaken)) continue;

            var local = new Vector3(pivot.x, _floorY - shape.min.y, pivot.y);
            if (accept != null && !accept(_wagon.TransformPoint(local))) continue;

            _floorTaken.Add(rect);
            obj.localPosition = local;
            return true;
        }
        return false;
    }

    public bool TryPlaceOnSurface(Transform obj, System.Random rng, System.Func<Vector3, bool> accept = null)
    {
        if (_surfaces.Count == 0) return false;
        var shape = Measure(obj);

        var order = new List<Surface>(_surfaces);
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        foreach (var surface in order)
        {
            for (int i = 0; i < Attempts / 3; i++)
            {
                if (!TryRandomPivot(surface.area, shape, rng, out var pivot, out var rect)) break;
                if (Overlaps(rect, surface.taken)) continue;
                if (!IsSupported(surface, rect)) continue;

                Vector3 local = new Vector3(pivot.x, surface.top - shape.min.y, pivot.y);
                if (accept != null && !accept(_wagon.TransformPoint(local))) continue;

                surface.taken.Add(rect);
                obj.localPosition = local;
                return true;
            }
        }
        return false;
    }

    private Bounds Measure(Transform obj)
    {
        obj.localPosition = Vector3.zero;
        return BoundsUtil.TryGetRendererBounds(obj.gameObject, out var world)
            ? BoundsUtil.ToLocal(_wagon, world)
            : new Bounds(Vector3.zero, Vector3.zero);
    }

    private static bool TryRandomPivot(Rect area, Bounds shape, System.Random rng, out Vector2 pivot, out Rect rect)
    {
        float xFrom = area.xMin - shape.min.x, xTo = area.xMax - shape.max.x;
        float zFrom = area.yMin - shape.min.z, zTo = area.yMax - shape.max.z;
        pivot = default;
        rect = default;
        if (xFrom > xTo || zFrom > zTo) return false;

        pivot = new Vector2(
            Mathf.Lerp(xFrom, xTo, (float)rng.NextDouble()),
            Mathf.Lerp(zFrom, zTo, (float)rng.NextDouble()));
        rect = new Rect(pivot.x + shape.min.x, pivot.y + shape.min.z, shape.size.x, shape.size.z);
        return true;
    }

    private bool IsSupported(Surface surface, Rect rect)
    {
        Rect outer = BoundsUtil.Expand(rect, SupportEdgeMargin);
        Vector2[] points =
        {
            outer.center,
            new Vector2(outer.xMin, outer.yMin),
            new Vector2(outer.xMax, outer.yMin),
            new Vector2(outer.xMin, outer.yMax),
            new Vector2(outer.xMax, outer.yMax)
        };

        foreach (Vector2 point in points)
        {
            if (!HitsSurfaceTop(surface, point))
                return false;
        }

        return true;
    }

    private bool HitsSurfaceTop(Surface surface, Vector2 point)
    {
        Vector3 origin = _wagon.TransformPoint(new Vector3(point.x, surface.top + SupportRayHeight, point.y));
        RaycastHit[] hits = Physics.RaycastAll(origin, -_wagon.up, SupportRayHeight + SupportTolerance, ~0, QueryTriggerInteraction.Ignore);

        Collider highest = null;
        float highestY = float.MinValue;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.transform.IsChildOf(_wagon))
                continue;

            float y = _wagon.InverseTransformPoint(hit.point).y;

            if (y > highestY)
            {
                highestY = y;
                highest = hit.collider;
            }
        }

        if (highest == null)
            return false;

        if (highest.GetComponentInParent<Furniture>() != surface.furniture)
            return false;

        return Mathf.Abs(highestY - surface.top) <= SupportTolerance;
    }

    private bool Overlaps(Rect rect, List<Rect> taken)
    {
        var padded = BoundsUtil.Expand(rect, _spacing);
        foreach (var t in taken)
        {
            if (padded.Overlaps(t)) return true;
        }
        return false;
    }
}
