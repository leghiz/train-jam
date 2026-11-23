using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Характеристики пули")]
    public float speed = 15f;
    public float lifetime = 5f;
    public int damage = 25;
    public string type;
    public BattleConfig config;

    private bool hasHit;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit)
            return;

        hasHit = true;

        if (type == "blue")
            HandleBlueHit(collision);
        else
            HandleRedHit(collision);

        Destroy(gameObject);
    }

    private void HandleBlueHit(Collision collision)
    {
        EnemyArmor enemy = collision.gameObject.GetComponentInParent<EnemyArmor>();
        if (enemy != null)
        {
            enemy.TakeHit(type, damage, collision.gameObject);
            return;
        }
    }

    private void HandleRedHit(Collision collision)
    {
        EnemyArmor enemy = collision.gameObject.GetComponentInParent<EnemyArmor>();
        if (enemy != null)
        {
            enemy.TakeHit(type, damage, collision.gameObject);
        }

        PlayerStats playerStats = collision.gameObject.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            PlayPlayerHit();
        }
    }

    private void PlayPlayerHit()
    {
        if (config == null)
            return;

        Camera camera = Camera.main;

        if (camera == null)
            return;

        if (config.bodyHitSound != null)
            AudioSource.PlayClipAtPoint(config.bodyHitSound, camera.transform.position, config.hitVolume);

        CameraShake shake = camera.GetComponent<CameraShake>();

        if (shake != null)
            shake.Shake(config.playerHitShake);
    }

    public void Init(Vector3 direction, string bulletType)
    {
        type = bulletType;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = direction.normalized * speed;
    }
}
