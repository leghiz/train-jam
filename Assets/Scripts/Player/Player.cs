using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Здоровье")]
    public int maxHealth = 100;
    public int health;

    void Awake()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"У игнрока осталось {health} HP");

        if (health <= 0) 
            Die();
    }

    private void Die()
    {
        Debug.Log("умер");
    }
}
