using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothTime = 0.3f;

    [SerializeField, Range(0.1f, 0.9f)]
    private float edgeThreshold = 0.4f;

    private Camera cam;
    private Vector3 velocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (player != null)
        {
            transform.position = new Vector3(
                player.position.x,
                transform.position.y,
                player.position.z
            );
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 viewport =
            cam.WorldToViewportPoint(player.position);

        float limit = (1f - edgeThreshold) / 2f;

        Vector2 correction = Vector2.zero;

        if (viewport.x < 0.5f - limit)
            correction.x = viewport.x - (0.5f - limit);
        else if (viewport.x > 0.5f + limit)
            correction.x = viewport.x - (0.5f + limit);

        if (viewport.y < 0.5f - limit)
            correction.y = viewport.y - (0.5f - limit);
        else if (viewport.y > 0.5f + limit)
            correction.y = viewport.y - (0.5f + limit);

        Vector3 movement =
            cam.transform.right * correction.x * 2f * cam.orthographicSize * cam.aspect +
            cam.transform.up * correction.y * 2f * cam.orthographicSize;

        Vector3 target = transform.position + movement;

        target.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target,
            ref velocity,
            smoothTime
        );
    }
}
