using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    [SerializeField] private float frequency = 22f;
    [SerializeField] private float maxAngle = 3f;
    [SerializeField] private float maxRoll = 2f;
    [SerializeField] private float maxOffset = 0.06f;
    [SerializeField] private float recovery = 2.2f;
    [SerializeField] private float kickAngle = 2.5f;
    [SerializeField] private float kickFov = 3f;
    [SerializeField] private float kickRecovery = 7f;

    private Camera cam;
    private float trauma;
    private float kick;
    private float savedFov;
    private float seed;
    private bool shaken;
    private Vector3 savedPosition;
    private Quaternion savedRotation;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        seed = Random.Range(0f, 100f);
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        RenderPipelineManager.endCameraRendering += OnEndCamera;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        RenderPipelineManager.endCameraRendering -= OnEndCamera;
        trauma = 0f;
    }

    public void Shake(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    public void Kick(float amount)
    {
        kick = Mathf.Clamp01(kick + amount);
    }

    private void Update()
    {
        trauma = Mathf.MoveTowards(trauma, 0f, recovery * Time.unscaledDeltaTime);
        kick = Mathf.MoveTowards(kick, 0f, kickRecovery * Time.unscaledDeltaTime);
    }

    private void OnBeginCamera(ScriptableRenderContext context, Camera camera)
    {
        if (camera != cam || (trauma <= 0f && kick <= 0f))
            return;

        float power = trauma * trauma;
        float time = Time.unscaledTime * frequency;

        float pitch = (Mathf.PerlinNoise(seed, time) * 2f - 1f) * maxAngle * power;
        float yaw = (Mathf.PerlinNoise(seed + 10f, time) * 2f - 1f) * maxAngle * power;
        float roll = (Mathf.PerlinNoise(seed + 20f, time) * 2f - 1f) * maxRoll * power;
        float offsetX = (Mathf.PerlinNoise(seed + 30f, time) * 2f - 1f) * maxOffset * power;
        float offsetY = (Mathf.PerlinNoise(seed + 40f, time) * 2f - 1f) * maxOffset * power;

        float recoil = kick * kick;

        savedPosition = transform.position;
        savedRotation = transform.rotation;
        savedFov = cam.fieldOfView;
        shaken = true;

        transform.position = savedPosition + transform.right * offsetX + transform.up * offsetY;
        transform.rotation = savedRotation * Quaternion.Euler(pitch - recoil * kickAngle, yaw, roll);
        cam.fieldOfView = savedFov + recoil * kickFov;
    }

    private void OnEndCamera(ScriptableRenderContext context, Camera camera)
    {
        if (!shaken || camera != cam)
            return;

        shaken = false;
        transform.SetPositionAndRotation(savedPosition, savedRotation);
        cam.fieldOfView = savedFov;
    }
}
