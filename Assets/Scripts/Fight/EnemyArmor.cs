using UnityEngine;

public class EnemyArmor : MonoBehaviour
{
    [Header("Здоровье")]
    public int health = 100;

    [Header("Конфиг")]
    public BattleConfig config;

    public void TakeHit(string type, int damage, GameObject hitObject)
    {
        Transform part = GetBodyPartRoot(hitObject);
        Armor armor = null;
        if (part != null)
            armor = part.GetComponentInChildren<Armor>();

        if (armor != null)
        {
            float armorMultiplier = config.normalArmorMultiplier;
            if (type == "blue")
                armorMultiplier = config.armorPiercingArmorMultiplier;

            armor.TakeHit((int)(armorMultiplier * damage));
            PlayHitSound(config.armorHitSound, hitObject);
            return;
        }

        PlayHitSound(config.bodyHitSound, hitObject);

        if (type == "blue")
        {
            health -= (int)(config.armorPiercingBodyMultiplier * damage);
            Debug.Log($"У {gameObject} осталось {health} HP");
        }

        else
        {
            health -= (int)(GetMultiplier(hitObject) * damage);
            Debug.Log($"У {gameObject} осталось {health} HP");

        }

        if (health <= 0) Die();
    }

    private void PlayHitSound(AudioClip clip, GameObject hitObject)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, hitObject.transform.position, config.hitVolume);
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private float GetMultiplier(GameObject hitObject)
    {
        switch (hitObject.tag)
        {
            case "Head": return config.headMultiplier;
            case "Body": return config.bodyMultiplier;
            case "Legs": return config.legsMultiplier;
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
