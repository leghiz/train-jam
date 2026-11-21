using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InventorySystem))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float pickupRadius = 2f;

    private InventorySystem inventory;

    private void Awake()
    {
        inventory = GetComponent<InventorySystem>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            PickUp();
    }

    private void PickUp()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius, ~0, QueryTriggerInteraction.Collide);

        ItemPickup nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            ItemPickup pickup = col.GetComponentInParent<ItemPickup>();
            if (pickup == null || pickup.Item == null)
                continue;

            float distance = Vector3.Distance(transform.position, pickup.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = pickup;
            }
        }

        if (nearest == null)
        {
            Debug.Log("Рядом нет предметов");
            return;
        }

        if (nearest.Item is WeaponData weapon)
        {
            SwapWeapon(nearest, weapon);
            return;
        }

        if (!inventory.Add(nearest.Item))
        {
            Debug.Log("Инвентарь полон");
            return;
        }

        Debug.Log($"Подобрано: {nearest.Item.displayName}");
        nearest.gameObject.SetActive(false);
        Destroy(nearest.gameObject);
    }

    private void SwapWeapon(ItemPickup pickup, WeaponData weapon)
    {
        Transform place = pickup.transform;
        WeaponData previous = inventory.EquipWeapon(weapon);

        if (previous != null && previous.worldPrefab != null)
            Instantiate(previous.worldPrefab, place.position, place.rotation, place.parent);

        Debug.Log(previous != null
            ? $"Взято оружие: {weapon.displayName}, оставлено: {previous.displayName}"
            : $"Взято оружие: {weapon.displayName}");

        pickup.gameObject.SetActive(false);
        Destroy(pickup.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
