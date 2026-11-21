using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Характеристики пули")]
    public float speed = 15f;
    public float lifetime = 5f;
    public int damage = 25;
    public string type;

    private void OnCollisionEnter(Collision collision)
    {
        if (type == "blue")
            HandleBlueHit(collision);
        else
            HandleRedHit(collision);

        Destroy(gameObject);
    }

    private void HandleBlueHit(Collision collision)
    {
        Armor armor = collision.gameObject.GetComponent<Armor>();
        if (armor != null)
        {
            armor.TakeHit();
            return;
        }

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

        Player player = collision.gameObject.GetComponentInParent<Player>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }

    public void Init(Vector3 direction, string bulletType)
    {
        type = bulletType;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = direction.normalized * speed;
    }
}
