
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private float pickupRadius = 2f;

    private ItemPickup[] slots = new ItemPickup[4];

    private int selectedSlot = 0;
    private bool inventoryOpen = false;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            inventoryOpen = !inventoryOpen;
            Debug.Log(inventoryOpen ? "Инвентарь открыт" : "Инвентарь закрыт");
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
            PickUpItem();

        if (!inventoryOpen)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectSlot(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectSlot(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectSlot(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            SelectSlot(3);

        if (Keyboard.current.qKey.wasPressedThisFrame)
            UseItem();

        if (Keyboard.current.gKey.wasPressedThisFrame)
            DropItem();
    }

    private void SelectSlot(int index)
    {
        selectedSlot = index;

        Debug.Log($"Выбрана ячейка {index + 1}");
    }

    
    private void PickUpItem()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            pickupRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        ItemPickup nearestItem = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            ItemPickup item = col.GetComponentInParent<ItemPickup>();

            if (item == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                item.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestItem = item;
            }
        }

        if (nearestItem == null)
        {
            Debug.Log("Рядом нет предметов!");
            return;
        }
        
        if (nearestItem.Type == ItemType.Key)
        {
            playerStats.TakeKey();
            Destroy(nearestItem.gameObject);
            return;
        }
        
        int emptySlot = System.Array.IndexOf(slots, null);

        if (emptySlot == -1)
        {
            Debug.Log("Инвентарь заполнен!");
            return;
        }

        slots[emptySlot] = nearestItem;
        nearestItem.gameObject.SetActive(false);

        Debug.Log($"Подобран предмет: {nearestItem.Type}");
    }


    private void UseItem()
    {
        ItemPickup item = slots[selectedSlot];

        if (item == null)
        {
            Debug.Log("Ячейка пуста!");
            return;
        }

        switch (item.Type)
        {
            case ItemType.Medkit:
                playerStats.Heal(40);
                break;

            case ItemType.Cigarettes:
                playerStats.ReduceStress(30);
                break;
        }

        Destroy(item.gameObject);
        slots[selectedSlot] = null;
    }

    private void DropItem()
    {
        ItemPickup item = slots[selectedSlot];

        if (item == null)
        {
            Debug.Log("Ячейка пуста!");
            return;
        }

        Vector3 randomDirection = Quaternion.Euler(
            0,
            Random.Range(0f, 360f),
            0
        ) * Vector3.forward;

        Vector3 dropPosition =
            transform.position + randomDirection * Random.Range(1.2f, 1.8f)
                               + Vector3.up * 0.2f;

        item.transform.position = dropPosition;
        item.gameObject.SetActive(true);

        slots[selectedSlot] = null;

        Debug.Log($"Выброшен предмет: {item.Type}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
