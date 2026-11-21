using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string type;
    public int damage = 25;

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
    }
}
