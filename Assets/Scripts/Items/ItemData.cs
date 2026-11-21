using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "TrainJam/Item")]
public class ItemData : PickupData
{
    [Header("Эффект")]
    public float heal;
    public float stressChange;
    public bool isKey;
    public bool consumeOnUse = true;
}
