using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ItemPreviewView : MonoBehaviour
{
    [SerializeField] private Vector2Int resolution = new Vector2Int(40, 40);
    [SerializeField] private float spinSpeed = 60f;
    [SerializeField] private bool spinAlways = true;
    [SerializeField] private float restAngle = 20f;
    [SerializeField] private float tilt = 20f;
    [SerializeField] private float padding = 1.1f;
    [SerializeField] private int rendererIndex = 1;
    [SerializeField] private string layerName = "ItemPreview";

    private static int stageCount;

    private RawImage image;
    private Camera stageCamera;
    private RenderTexture texture;
    private Transform stage;
    private Transform pivot;
    private GameObject model;
    private PickupData shown;
    private float angle;
    private float baseAngle;
    private bool spinning;

    public bool HasModel
    {
        get { return model != null; }
    }

    public void SetSpinning(bool value)
    {
        spinning = value;
    }

    private void OnEnable()
    {
        if (stageCamera != null)
            stageCamera.enabled = model != null;
    }

    private void OnDisable()
    {
        if (stageCamera != null)
            stageCamera.enabled = false;
    }

    private void Update()
    {
        if (model == null)
            return;

        if (spinAlways || spinning)
            angle = Mathf.Repeat(angle + spinSpeed * Time.unscaledDeltaTime, 360f);
        else
            angle = Mathf.MoveTowardsAngle(angle, 0f, spinSpeed * 4f * Time.unscaledDeltaTime);

        pivot.localRotation = Quaternion.Euler(tilt, 0f, 0f) * Quaternion.Euler(0f, baseAngle + restAngle + angle, 0f);
    }

    public void Show(PickupData item)
    {
        if (stage != null && item == shown && (item == null || model != null))
            return;

        shown = item;
        CreateStage();

        if (model != null)
            Destroy(model);

        model = null;

        if (item == null || item.worldPrefab == null)
        {
            image.enabled = false;
            stageCamera.enabled = false;
            return;
        }

        model = CreateModel(item.worldPrefab, Quaternion.Euler(item.previewRotation));
        image.enabled = true;
        stageCamera.enabled = isActiveAndEnabled;
    }

    private void CreateStage()
    {
        if (stage != null)
            return;

        image = GetComponent<RawImage>();

        stage = new GameObject("ItemPreviewStage").transform;
        stage.position = new Vector3(stageCount * 20f, -1000f, 0f);
        stageCount++;

        pivot = new GameObject("Pivot").transform;
        pivot.SetParent(stage, false);

        texture = new RenderTexture(resolution.x, resolution.y, 16, RenderTextureFormat.ARGB32);
        texture.filterMode = FilterMode.Point;
        image.texture = texture;

        GameObject cameraObject = new GameObject("Camera");
        cameraObject.transform.SetParent(stage, false);

        stageCamera = cameraObject.AddComponent<Camera>();
        stageCamera.orthographic = true;
        stageCamera.clearFlags = CameraClearFlags.SolidColor;
        stageCamera.backgroundColor = Color.clear;
        stageCamera.cullingMask = 1 << LayerMask.NameToLayer(layerName);
        stageCamera.allowHDR = false;
        stageCamera.allowMSAA = false;
        stageCamera.targetTexture = texture;
        stageCamera.enabled = false;

        UniversalAdditionalCameraData data = stageCamera.GetUniversalAdditionalCameraData();
        data.SetRenderer(rendererIndex);
        data.renderPostProcessing = false;
        data.renderShadows = false;
    }

    private GameObject CreateModel(GameObject source, Quaternion rotation)
    {
        pivot.localRotation = Quaternion.identity;

        GameObject copy = Instantiate(source, pivot);
        copy.transform.localPosition = Vector3.zero;
        copy.transform.localRotation = rotation;

        foreach (MonoBehaviour behaviour in copy.GetComponentsInChildren<MonoBehaviour>(true))
        {
            DestroyImmediate(behaviour);
        }

        foreach (Collider col in copy.GetComponentsInChildren<Collider>(true))
        {
            DestroyImmediate(col);
        }

        foreach (Rigidbody body in copy.GetComponentsInChildren<Rigidbody>(true))
        {
            DestroyImmediate(body);
        }

        int layer = LayerMask.NameToLayer(layerName);

        foreach (Transform part in copy.GetComponentsInChildren<Transform>(true))
        {
            part.gameObject.layer = layer;
        }

        Renderer[] renderers = copy.GetComponentsInChildren<Renderer>();
        bool hasBounds = false;
        Bounds bounds = new Bounds();

        foreach (Renderer r in renderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            if (!r.enabled)
                continue;

            Bounds part = GetTightBounds(r);

            if (hasBounds)
            {
                bounds.Encapsulate(part);
            }
            else
            {
                bounds = part;
                hasBounds = true;
            }
        }

        if (!hasBounds)
            return copy;

        copy.transform.position -= bounds.center - pivot.position;

        float radius = Mathf.Max(bounds.extents.magnitude, 0.01f);
        float aspect = (float)resolution.x / resolution.y;
        float flatRadius = new Vector2(bounds.extents.x, bounds.extents.z).magnitude;
        float tiltRadians = tilt * Mathf.Deg2Rad;
        float halfHeight = bounds.extents.y * Mathf.Cos(tiltRadians) + flatRadius * Mathf.Abs(Mathf.Sin(tiltRadians));

        baseAngle = 0f;

        if (bounds.extents.z > bounds.extents.x)
            baseAngle = 90f;

        angle = 0f;
        pivot.localRotation = Quaternion.Euler(tilt, 0f, 0f) * Quaternion.Euler(0f, baseAngle + restAngle, 0f);

        stageCamera.orthographicSize = Mathf.Max(halfHeight, flatRadius / aspect, 0.01f) * padding;
        stageCamera.transform.localPosition = new Vector3(0f, 0f, -radius - 1f);
        stageCamera.transform.localRotation = Quaternion.identity;
        stageCamera.nearClipPlane = 0.01f;
        stageCamera.farClipPlane = radius * 2f + 2f;

        return copy;
    }

    private Bounds GetTightBounds(Renderer target)
    {
        MeshFilter filter = target.GetComponent<MeshFilter>();

        if (filter == null || filter.sharedMesh == null || !filter.sharedMesh.isReadable)
            return target.bounds;

        Vector3[] vertices = filter.sharedMesh.vertices;

        if (vertices.Length == 0)
            return target.bounds;

        Transform part = filter.transform;
        Bounds result = new Bounds(part.TransformPoint(vertices[0]), Vector3.zero);

        for (int i = 1; i < vertices.Length; i++)
        {
            result.Encapsulate(part.TransformPoint(vertices[i]));
        }

        return result;
    }

    private void OnDestroy()
    {
        if (stage != null)
            Destroy(stage.gameObject);

        if (texture != null)
            texture.Release();
    }
}
