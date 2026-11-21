using UnityEngine;

public enum ItemType
{
    Medkit,
    Cigarettes,
    Key
}

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType;

    public ItemType Type => itemType;
}