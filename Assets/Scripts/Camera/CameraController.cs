using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothTime = 0.3f;

    [SerializeField, Range(0.1f, 0.9f)]
    private float edgeThreshold = 0.4f;

    [SerializeField, Range(20f, 90f)] private float pitch = 60f;
    [SerializeField] private float distance = 24f;
    [SerializeField] private float fieldOfView = 20f;

    [SerializeField] private float leadSeconds = 0.4f;
    [SerializeField] private float maxLead = 1.2f;
    [SerializeField] private float leadSharpness = 1.5f;

    private Camera cam;
    private Vector3 velocity;
    private Vector3 target;
    private Vector3 lead;
    private Vector3 lastPlayerPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = fieldOfView;
        transform.rotation = Quaternion.Euler(pitch, 0f, 0f);

        if (player != null)
        {
            transform.position = new Vector3(
                player.position.x,
                transform.position.y,
                player.position.z
            );

            transform.position = player.position - transform.forward * distance;
            lastPlayerPosition = player.position;
        }

        target = transform.position;
    }

    private void LateUpdate()
    {
        if (player == null || Time.deltaTime <= 0f)
            return;

        Vector3 playerVelocity = (player.position - lastPlayerPosition) / Time.deltaTime;
        playerVelocity.y = 0f;
        lastPlayerPosition = player.position;

        float leadBlend = 1f - Mathf.Exp(-leadSharpness * Time.deltaTime);
        lead = Vector3.Lerp(lead, Vector3.ClampMagnitude(playerVelocity * leadSeconds, maxLead), leadBlend);

        Vector3 viewport =
            cam.WorldToViewportPoint(player.position + lead + transform.position - target);

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

        float frameHeight = 2f * viewport.z * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frameWidth = frameHeight * cam.aspect;

        Vector3 flatUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up);

        Vector3 movement =
            cam.transform.right * correction.x * frameWidth +
            flatUp.normalized * correction.y * frameHeight / flatUp.magnitude;

        target += movement;

        target.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target,
            ref velocity,
            smoothTime
        );
    }
}
