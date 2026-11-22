using UnityEngine;

public class Armor : MonoBehaviour
{
    [Header("’п брони")]
    public int durability = 40;

    public void TakeHit(int damage)
    {
        durability -= damage;

        if (durability <= 0)
            Destroy(gameObject);
    }
}
