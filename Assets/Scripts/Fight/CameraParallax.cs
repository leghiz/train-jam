using UnityEngine;
using UnityEngine.InputSystem;

public class CameraParallax : MonoBehaviour
{
    [Header("Parallax")]
    [SerializeField] private float maxYaw = 3f;
    [SerializeField] private float maxPitch = 2f;
    [SerializeField] private float smoothness = 5f;

    [Header("Camera Oscillation")]
    [SerializeField] private float shakeSpeed = 2f;
    [SerializeField] private float blendSpeed = 5f;

    private float targetAmplitude;
    private float currentAmplitude;
    private float time;

    private Quaternion baseRotation;
    private Quaternion currentParallaxRotation;

    private void Awake()
    {
        baseRotation = transform.localRotation;
        currentParallaxRotation = baseRotation;
    }

    public void SetWeapon(WeaponData weapon)
    {
        targetAmplitude = weapon.aimingOscillation;
    }

    private void LateUpdate()
    {
        UpdateParallax();
        UpdateOscillation();
    }

    private void UpdateParallax()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector2 position = mouse.position.ReadValue();

        float x = Mathf.Clamp(position.x / Screen.width * 2f - 1f, -1f, 1f);
        float y = Mathf.Clamp(position.y / Screen.height * 2f - 1f, -1f, 1f);

        Quaternion targetRotation = baseRotation * Quaternion.Euler(-y * maxPitch, x * maxYaw, 0f);

        currentParallaxRotation = Quaternion.Slerp(currentParallaxRotation, targetRotation, smoothness * Time.deltaTime);
    }

    private void UpdateOscillation()
    {
        float t = 1f - Mathf.Exp(-blendSpeed * Time.deltaTime);

        currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, t);

        time += Time.deltaTime * shakeSpeed;

        float nx = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
        float ny = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f;

        Quaternion shakeRotation = Quaternion.Euler(ny * currentAmplitude, nx * currentAmplitude,  0f);

        transform.localRotation = currentParallaxRotation * shakeRotation;
    }
}
