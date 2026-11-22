using UnityEngine;
using UnityEngine.InputSystem;

public class CameraParallax : MonoBehaviour
{
    [SerializeField] private float maxYaw = 3f;
    [SerializeField] private float maxPitch = 2f;
    [SerializeField] private float smoothness = 5f;

    private Quaternion baseRotation;

    private void Start()
    {
        baseRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector2 position = mouse.position.ReadValue();
        float x = Mathf.Clamp(position.x / Screen.width * 2f - 1f, -1f, 1f);
        float y = Mathf.Clamp(position.y / Screen.height * 2f - 1f, -1f, 1f);

        Quaternion target = baseRotation * Quaternion.Euler(-y * maxPitch, x * maxYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, smoothness * Time.deltaTime);
    }
}
