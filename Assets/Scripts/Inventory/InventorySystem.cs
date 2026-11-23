using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField, Min(1)] private int slotCount = 4;
    [SerializeField] private WeaponData startingWeapon;
    [SerializeField] private float dropDistance = 1.2f;
    [SerializeField] private float dropRingStep = 0.4f;
    [SerializeField] private int dropRings = 3;
    [SerializeField] private float dropAngleStep = 30f;
    [SerializeField] private float dropFloorTolerance = 0.15f;

    private ItemData[] slots;
    private TrainGenerator generator;

    public event Action Changed;

    public IReadOnlyList<ItemData> Slots => slots;
    public int SelectedSlot { get; private set; }
    public WeaponData Weapon { get; private set; }

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        slots = new ItemData[slotCount];
        Weapon = startingWeapon;
    }

    private void Start()
    {
        generator = FindFirstObjectByType<TrainGenerator>();
    }

    public WeaponData EquipWeapon(WeaponData weapon)
    {
        WeaponData previous = Weapon;
        Weapon = weapon;
        WeaponProgress.Unlock(weapon);
        Changed?.Invoke();
        return previous;
    }

    public bool Add(ItemData item)
    {
        if (item == null)
            return false;

        if (item.isKey)
        {
            playerStats.TakeKey();
            return true;
        }

        int index = Array.IndexOf(slots, null);
        if (index < 0)
            return false;

        slots[index] = item;
        Changed?.Invoke();
        return true;
    }

    public void Select(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        SelectedSlot = index;
        Changed?.Invoke();
    }

    public bool Use(int index)
    {
        return Use(index, playerStats);
    }

    public bool Use(int index, PlayerStats target)
    {
        ItemData item = Get(index);
        if (item == null)
            return false;

        if (item.heal > 0f)
            target.Heal(item.heal);

        if (item.stressChange < 0f)
            target.ReduceStress(-item.stressChange);
        else if (item.stressChange > 0f)
            target.AddStress(item.stressChange);

        if (item.consumeOnUse)
            Remove(index);

        return true;
    }

    public bool Drop(int index)
    {
        ItemData item = Get(index);

        if (item == null || item.worldPrefab == null)
            return false;
        
        Transform parent = generator != null && generator.CurrentWagon != null
            ? generator.CurrentWagon.transform
            : null;

        GameObject dropped = Instantiate(item.worldPrefab, transform.position, Quaternion.identity, parent);
        dropped.transform.position = FindDropPoint(dropped);

        Remove(index);

        return true;
    }

    private Vector3 FindDropPoint(GameObject dropped)
    {
        Bounds bounds;

        if (!TryGetBounds(dropped, out bounds))
            return transform.position;

        Vector3 pivot = dropped.transform.position;
        Vector3 centerOffset = bounds.center - pivot;
        float bottomOffset = pivot.y - bounds.min.y;
        Vector3 halfSize = bounds.extents + new Vector3(0.02f, 0f, 0.02f);

        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 start = new Vector3(randomDirection.x, 0f, randomDirection.y);

        if (start.sqrMagnitude < 0.001f)
            start = Vector3.forward;

        for (int ring = 0; ring < dropRings; ring++)
        {
            float distance = dropDistance + ring * dropRingStep;

            for (int step = 0; step * dropAngleStep <= 180f; step++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    if (side > 0 && (step == 0 || Mathf.Approximately(step * dropAngleStep, 180f)))
                        continue;

                    float angle = step * dropAngleStep * side;
                    Vector3 candidate = transform.position + Quaternion.Euler(0f, angle, 0f) * start * distance;
                    Vector3 point;

                    if (TryGetFreePoint(dropped, candidate, centerOffset, bottomOffset, halfSize, out point))
                        return point;
                }
            }
        }

        return new Vector3(transform.position.x, transform.position.y + bottomOffset, transform.position.z);
    }

    private bool TryGetFreePoint(GameObject dropped, Vector3 candidate, Vector3 centerOffset, float bottomOffset, Vector3 halfSize, out Vector3 point)
    {
        point = candidate;

        Vector3 eye = transform.position + Vector3.up;
        Vector3 above = candidate + Vector3.up;

        if (IsBlocked(eye, above, dropped))
            return false;

        RaycastHit hit;

        if (!Physics.Raycast(above, Vector3.down, out hit, 2f, ~0, QueryTriggerInteraction.Ignore))
            return false;

        if (Mathf.Abs(hit.point.y - transform.position.y) > dropFloorTolerance)
            return false;

        point = new Vector3(candidate.x, hit.point.y + bottomOffset, candidate.z);
        Vector3 center = point + centerOffset + Vector3.up * 0.05f;
        Collider[] hits = Physics.OverlapBox(center, halfSize, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider col in hits)
        {
            if (col.transform.IsChildOf(transform) || col.transform.IsChildOf(dropped.transform))
                continue;

            return false;
        }

        return true;
    }

    private bool IsBlocked(Vector3 from, Vector3 to, GameObject dropped)
    {
        Vector3 direction = to - from;
        RaycastHit[] hits = Physics.RaycastAll(from, direction.normalized, direction.magnitude, ~0, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform.IsChildOf(dropped.transform))
                continue;

            return true;
        }

        return false;
    }

    private bool TryGetBounds(GameObject target, out Bounds bounds)
    {
        bounds = new Bounds();
        bool hasBounds = false;

        foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled)
                continue;

            if (hasBounds)
            {
                bounds.Encapsulate(r.bounds);
            }
            else
            {
                bounds = r.bounds;
                hasBounds = true;
            }
        }

        return hasBounds;
    }

    public ItemData Remove(int index)
    {
        ItemData item = Get(index);
        if (item == null)
            return null;

        slots[index] = null;
        Changed?.Invoke();
        return item;
    }

    public bool Has(ItemData item)
    {
        return item != null && Array.IndexOf(slots, item) >= 0;
    }

    public void ResetInventory()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = null;
        }

        SelectedSlot = 0;
        Weapon = startingWeapon;
        Changed?.Invoke();

        Debug.Log("Инвентарь очищен!");
    }

    private ItemData Get(int index)
    {
        return index >= 0 && index < slots.Length ? slots[index] : null;
    }
}
