using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class HeroFogView : MonoBehaviour
{
    private static readonly int CenterId = Shader.PropertyToID("_HeroFogCenter");
    private static readonly int ColorId = Shader.PropertyToID("_HeroFogColor");
    private static readonly int RangeId = Shader.PropertyToID("_HeroFogRange");
    private static readonly int AimId = Shader.PropertyToID("_HeroFogAim");

    [SerializeField] private Color color = Color.black;
    [SerializeField, Min(0f)] private float clearRadius = 0.5f;
    [SerializeField, Min(0.1f)] private float fullRadius = 2.96f;
    [SerializeField, Range(0f, 1f)] private float density = 0.882f;

    [Header("Aim")]
    [SerializeField, Min(1f)] private float aimStretch = 2f;
    [SerializeField, Min(0.01f)] private float aimSeconds = 0.35f;

    private bool isAiming;
    private float aimed;

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    private void LateUpdate()
    {
        if (gameObject.scene != SceneManager.GetActiveScene())
            return;

        Shader.SetGlobalVector(CenterId, transform.position);
        Shader.SetGlobalColor(ColorId, color);
        Shader.SetGlobalVector(RangeId, new Vector4(clearRadius, Mathf.Max(fullRadius, clearRadius + 0.1f), density, 0f));

        float aimTarget = 0f;

        if (isAiming)
            aimTarget = 1f;

        if (Application.isPlaying)
            aimed = Mathf.MoveTowards(aimed, aimTarget, Time.deltaTime / aimSeconds);
        else
            aimed = 0f;

        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Shader.SetGlobalVector(AimId, new Vector4(forward.x, forward.z, Mathf.Lerp(1f, aimStretch, Mathf.SmoothStep(0f, 1f, aimed)), 0f));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        ResetFog();
    }

    private void OnDisable()
    {
        ResetFog();
    }

    private static void ResetFog()
    {
        Shader.SetGlobalVector(RangeId, Vector4.zero);
        Shader.SetGlobalVector(AimId, Vector4.zero);
    }
}
