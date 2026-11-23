using UnityEngine;

public class Armor : MonoBehaviour
{
    [Header("’п брони")]
    public int durability = 40;
    public Armor pair;

    public void TakeHit(int damage)
    {
        durability -= damage;

        if (pair != null)
            pair.durability = durability;

        if (durability > 0)
            return;

        if (pair != null)
            Destroy(pair.gameObject);

        Destroy(gameObject);
    }
}
