using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private PickupData item;

    public PickupData Item => item;
}
