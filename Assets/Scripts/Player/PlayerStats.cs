
using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStress = 100f;

    public float Health { get; private set; } = 100f;
    public float Stress { get; private set; } = 0f;
    public bool HasKey { get; private set; }

    public event Action Died;

    private bool isDead;

    public void TakeKey()
    {
        HasKey = true;
        Debug.Log("Ключ получен!");
    }

    public void Heal(float amount)
    {
        Health = Mathf.Clamp(Health + amount, 0, maxHealth);
        Debug.Log($"Здоровье: {Health}");
    }

    public void ReduceStress(float amount)
    {
        Stress = Mathf.Clamp(Stress - amount, 0, maxStress);
        Debug.Log($"Стресс: {Stress}");
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;

        Health -= amount;
        Debug.Log($"Здоровье: {Health}");

        if (Health <= 0)
        {
            isDead = true;
            Debug.Log("Игрок погиб!");
            Died?.Invoke();
        }
    }

    public void AddStress(float amount)
    {
        Stress = Mathf.Clamp(Stress + amount, 0, maxStress);
        Debug.Log($"Стресс: {Stress}");
    }
    
    public void CopyFrom(PlayerStats source)
    {
        if (source == null)
            return;

        Health = Mathf.Clamp(source.Health, 0, maxHealth);
        Stress = Mathf.Clamp(source.Stress, 0, maxStress);
    }

    public void ResetStats()
    {
        Health = maxHealth;
        Stress = 0;
        HasKey = false;
        isDead = false;

        Debug.Log("Характеристики игрока восстановлены!");
    }
}
