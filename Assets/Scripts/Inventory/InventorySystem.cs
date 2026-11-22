using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField, Min(1)] private int slotCount = 4;
    [SerializeField] private WeaponData startingWeapon;
    [SerializeField] private float dropDistance = 1.2f;

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

        Remove(index);
        Vector3 position = transform.position + transform.forward * dropDistance;
        Transform parent = generator != null && generator.CurrentWagon != null ? generator.CurrentWagon.transform : null;
        Instantiate(item.worldPrefab, position, Quaternion.identity, parent);
        return true;
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
