using UnityEngine;

public class TargetArrowView : MonoBehaviour
{
    [SerializeField] private PlayerCombatTargeting targeting;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private float offset = 10f;
    [SerializeField] private float bounceHeight = 8f;
    [SerializeField] private float bouncesPerSecond = 2f;

    private RectTransform area;

    private void Awake()
    {
        area = transform as RectTransform;
        arrow.gameObject.SetActive(false);

        if (targeting == null)
            targeting = FindAnyObjectByType<PlayerCombatTargeting>();
    }

    private void LateUpdate()
    {
        EnemyEncounter target = null;

        if (targeting != null && targeting.IsAiming && !BattleContext.HasEncounter)
            target = targeting.CurrentTarget;

        Camera cam = Camera.main;
        Rect screenRect = new Rect();
        bool isVisible = target != null && cam != null && TryGetScreenRect(cam, target.gameObject, out screenRect);

        arrow.gameObject.SetActive(isVisible);

        if (!isVisible)
            return;

        float bounce = Mathf.Abs(Mathf.Sin(Time.time * bouncesPerSecond * Mathf.PI)) * bounceHeight;
        arrow.anchoredPosition = ToLocal(new Vector2(screenRect.center.x, screenRect.yMax)) + new Vector2(0f, offset + bounce);
    }

    private bool TryGetScreenRect(Camera cam, GameObject target, out Rect screenRect)
    {
        screenRect = new Rect();
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return false;

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? bounds.min.x : bounds.max.x,
                (i & 2) == 0 ? bounds.min.y : bounds.max.y,
                (i & 4) == 0 ? bounds.min.z : bounds.max.z);

            Vector3 point = cam.WorldToScreenPoint(corner);

            if (point.z <= 0f)
                return false;

            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }

        screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }

    private Vector2 ToLocal(Vector2 screenPoint)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPoint, null, out local);
        return local;
    }
}
