using UnityEngine;

public class EnemyArmor : MonoBehaviour
{
    [Header("Здоровье")]
    public int health = 100;

    [Header("Множители урона по частям тела")]
    public float headMultiplier = 1.2f;
    public float bodyMultiplier = 1f;
    public float legsMultiplier = 0.7f;

    public void TakeHit(string type, int damage, GameObject hitObject)
    {
        if (type == "blue")
        {
            health -= (int)(0.1f * damage);
            Debug.Log($"У {gameObject} осталось {health} HP");
        }

        else
        {
            Transform part = GetBodyPartRoot(hitObject);
            if (part.GetComponentsInChildren<Armor>().Length > 0)
                return;

            health -= (int)(GetMultiplier(hitObject) * damage);
            Debug.Log($"У {gameObject} осталось {health} HP");

        }

        if (health <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private float GetMultiplier(GameObject hitObject)
    {
        switch (hitObject.tag)
        {
            case "Head": return headMultiplier;
            case "Body": return bodyMultiplier;
            case "Legs": return legsMultiplier;
            default: return 1f;
        }
    }

    private Transform GetBodyPartRoot(GameObject hitObject)
    {
        Transform transform = hitObject.transform;
        while (transform != null && transform != base.transform)
        {
            if (transform.CompareTag("Head") || transform.CompareTag("Body") || transform.CompareTag("Legs"))
                return transform;
            transform = transform.parent;
        }
        return null;
    }
}
