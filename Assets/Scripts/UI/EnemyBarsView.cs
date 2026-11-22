using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EnemyBarsView : MonoBehaviour
{
    [SerializeField] private RectTransform healthBarTemplate;
    [SerializeField] private RectTransform armorBar;
    [SerializeField] private Image armorFill;
    [SerializeField] private InventoryView inventoryView;
    [SerializeField] private float healthOffset = 40f;
    [SerializeField] private float armorOffset = 24f;

    private RectTransform area;
    private Dictionary<EnemyArmor, RectTransform> bars = new Dictionary<EnemyArmor, RectTransform>();
    private Dictionary<EnemyArmor, int> maxHealth = new Dictionary<EnemyArmor, int>();
    private Dictionary<Armor, int> maxDurability = new Dictionary<Armor, int>();
    private List<EnemyArmor> removed = new List<EnemyArmor>();

    private void Awake()
    {
        area = transform as RectTransform;
        healthBarTemplate.gameObject.SetActive(false);
        armorBar.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        bool isHidden = inventoryView.IsOpen;

        UpdateHealthBars(cam, isHidden);
        UpdateArmorBar(cam, isHidden);
    }

    private void UpdateHealthBars(Camera cam, bool isHidden)
    {
        EnemyArmor[] enemies = FindObjectsByType<EnemyArmor>();

        foreach (EnemyArmor enemy in enemies)
        {
            if (enemy.gameObject.scene != gameObject.scene)
                continue;

            if (!bars.ContainsKey(enemy))
            {
                GameObject bar = Instantiate(healthBarTemplate.gameObject, healthBarTemplate.parent);
                bars.Add(enemy, bar.GetComponent<RectTransform>());
                maxHealth.Add(enemy, Mathf.Max(1, enemy.health));
            }

            RectTransform rect = bars[enemy];
            Rect screenRect = new Rect();
            bool isVisible = !isHidden && TryGetScreenRect(cam, enemy.gameObject, out screenRect);
            rect.gameObject.SetActive(isVisible);

            if (!isVisible)
                continue;

            rect.anchoredPosition = ToLocal(new Vector2(screenRect.center.x, screenRect.yMax)) + new Vector2(0f, healthOffset);
            rect.Find("Fill").GetComponent<Image>().fillAmount = Mathf.Clamp01((float)enemy.health / maxHealth[enemy]);
        }

        removed.Clear();

        foreach (KeyValuePair<EnemyArmor, RectTransform> pair in bars)
        {
            if (pair.Key == null)
                removed.Add(pair.Key);
        }

        foreach (EnemyArmor enemy in removed)
        {
            Destroy(bars[enemy].gameObject);
            bars.Remove(enemy);
            maxHealth.Remove(enemy);
        }
    }

    private void UpdateArmorBar(Camera cam, bool isHidden)
    {
        Armor armor = null;
        Mouse mouse = Mouse.current;

        if (!isHidden && mouse != null)
        {
            Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, ~0, QueryTriggerInteraction.Ignore))
                armor = hit.collider.GetComponent<Armor>();
        }

        Rect screenRect = new Rect();

        if (armor == null || !TryGetScreenRect(cam, armor.gameObject, out screenRect))
        {
            armorBar.gameObject.SetActive(false);
            return;
        }

        if (!maxDurability.ContainsKey(armor))
            maxDurability.Add(armor, Mathf.Max(1, armor.durability));

        armorBar.gameObject.SetActive(true);
        armorBar.anchoredPosition = ToLocal(new Vector2(screenRect.xMax, screenRect.center.y)) + new Vector2(armorOffset, 0f);
        armorFill.fillAmount = Mathf.Clamp01((float)armor.durability / maxDurability[armor]);
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
