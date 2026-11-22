using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "TrainJam/Item")]
public class ItemData : PickupData
{
    [Header("Описание")]
    [TextArea] public string description;

    [Header("Эффект")]
    public float heal;
    public float stressChange;
    public bool isKey;
    public bool consumeOnUse = true;
}
