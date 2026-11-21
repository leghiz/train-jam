using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStress = 100f;

    public float Health { get; private set; } = 100f;
    public float Stress { get; private set; } = 0f;

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
        Health = Mathf.Clamp(Health - amount, 0, maxHealth);
        Debug.Log($"Здоровье: {Health}");
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
}