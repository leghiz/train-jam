using UnityEngine;

public class Armor : MonoBehaviour
{
    [Header("’п брони")]
    public int durability = 40;

    [Header("ћножители урона по броне")]
    public float armorPiercingMultiplier = 1f;
    public float normalMultiplier = 0.4f;

    public void TakeHit(string type, int damage)
    {
        if (type == "blue")
            durability -= (int)(armorPiercingMultiplier * damage);
        else
            durability -= (int)(normalMultiplier * damage);

        if (durability <= 0)
            Destroy(gameObject);
    }
}
