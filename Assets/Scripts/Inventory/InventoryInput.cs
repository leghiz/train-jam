using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InventorySystem))]
public class InventoryInput : MonoBehaviour
{
    private InventorySystem inventory;
    private bool open;

    private void Awake()
    {
        inventory = GetComponent<InventorySystem>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            open = !open;
            Debug.Log(open ? $"Инвентарь открыт: {SlotsText()}" : "Инвентарь закрыт");
        }

        if (!open)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame) Select(0);
        if (keyboard.digit2Key.wasPressedThisFrame) Select(1);
        if (keyboard.digit3Key.wasPressedThisFrame) Select(2);
        if (keyboard.digit4Key.wasPressedThisFrame) Select(3);

        if (keyboard.qKey.wasPressedThisFrame)
            Use();

        if (keyboard.gKey.wasPressedThisFrame)
            Drop();
    }

    private void Select(int index)
    {
        inventory.Select(index);
        Debug.Log($"Ячейка {inventory.SelectedSlot + 1}: {NameOf(inventory.Slots[inventory.SelectedSlot])}");
    }

    private void Use()
    {
        ItemData item = inventory.Slots[inventory.SelectedSlot];
        Debug.Log(inventory.Use(inventory.SelectedSlot)
            ? $"Использовано: {item.displayName}"
            : $"Ячейка {inventory.SelectedSlot + 1} пуста");
    }

    private void Drop()
    {
        ItemData item = inventory.Slots[inventory.SelectedSlot];
        Debug.Log(inventory.Drop(inventory.SelectedSlot)
            ? $"Выброшено: {item.displayName}"
            : $"Ячейка {inventory.SelectedSlot + 1}: нечего выбросить");
    }

    private string SlotsText()
    {
        string[] names = new string[inventory.Slots.Count];
        for (int i = 0; i < names.Length; i++)
            names[i] = $"{i + 1}: {NameOf(inventory.Slots[i])}";
        return string.Join(", ", names) + $" | в руках: {NameOf(inventory.Weapon)}";
    }

    private static string NameOf(PickupData item)
    {
        return item != null ? item.displayName : "—";
    }
}
