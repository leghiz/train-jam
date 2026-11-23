using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "TrainJam/Weapon")]
public class WeaponData : PickupData
{
    [Header("Магазин")]
    public int magazineSize = 1;
    public float reloadTime = 1.5f;

    [Header("Прицеливание")]
    public float aimingSensitivity = 0.1f;
    public float aimingOscillation = 1f;

    [Header("Урон")]
    public int damage = 20;
}
