using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData item;

    public ItemData Item => item;
}
