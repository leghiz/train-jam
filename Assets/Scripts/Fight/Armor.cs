using UnityEngine;

public class Armor : MonoBehaviour
{
    [Header("’п брони")]
    public int hitsToDestroy = 1;

    private int currentHits = 0;

    public void TakeHit()
    {
        currentHits++;
        if (currentHits >= hitsToDestroy)
            Destroy(gameObject);
    }
}
