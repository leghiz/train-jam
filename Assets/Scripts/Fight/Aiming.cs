using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class Aiming : MonoBehaviour
{
    [Header("Чувствительность")]
    public float sensitivity = 0.1f;

    [Header("Вертикальные ограничения")]
    public float minPitch = -5f;
    public float maxPitch = 10f;

    private float deviation = 0f;
    private float currentAngleY = 0f;
    private WeaponData weapon;

    void Start()
    {
        weapon = BattleContext.Weapon;

        if (weapon != null)
        {

            Debug.Log($"Оружие: {weapon.displayName}, чувствительность: {sensitivity}");
        }
        else
        {
            Debug.LogWarning("Aiming: оружие не найдено!");
        }

        deviation = transform.localEulerAngles.y;
        currentAngleY = 0f;
    }

    void Update()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * sensitivity;
        float mouseY = Mouse.current.delta.y.ReadValue() * sensitivity;

        deviation += mouseX;
        currentAngleY = Mathf.Clamp(currentAngleY - mouseY, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(currentAngleY, deviation, 0f);
    }
}
