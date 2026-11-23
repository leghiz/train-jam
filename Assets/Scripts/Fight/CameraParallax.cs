using UnityEngine;
using UnityEngine.InputSystem;

public class CameraParallax : MonoBehaviour
{
    [Header("Parallax")]
    [SerializeField] private float maxYaw = 3f;
    [SerializeField] private float maxPitch = 2f;
    [SerializeField] private float smoothness = 5f;

    private Quaternion baseRotation;
    private Quaternion currentParallaxRotation;

    private void Awake()
    {
        baseRotation = transform.localRotation;
        currentParallaxRotation = baseRotation;
    }

    private void LateUpdate()
    {
        UpdateParallax();
    }

    private void UpdateParallax()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector2 position = mouse.position.ReadValue();

        float x = Mathf.Clamp(position.x / Screen.width * 2f - 1f, -1f, 1f);
        float y = Mathf.Clamp(position.y / Screen.height * 2f - 1f, -1f, 1f);

        Quaternion targetRotation = baseRotation *
            Quaternion.Euler(-y * maxPitch, x * maxYaw, 0f);

        currentParallaxRotation = Quaternion.Slerp(
            currentParallaxRotation,
            targetRotation,
            smoothness * Time.deltaTime
        );

        transform.localRotation = currentParallaxRotation;
    }
}
