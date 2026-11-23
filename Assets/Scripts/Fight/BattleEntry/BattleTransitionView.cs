using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class BattleTransitionView : MonoBehaviour
{
    private const float MaxStep = 1f / 30f;

    private static readonly int BlurId = Shader.PropertyToID("_BattleBlur");
    private static readonly int FlashId = Shader.PropertyToID("_BattleFlash");
    private static readonly int CenterId = Shader.PropertyToID("_BattleCenter");

    public event Action Covered;

    private BattleTransitionConfig config;
    private Camera eye;
    private Camera revealEye;
    private float revealZoom;
    private List<CanvasGroup> hudGroups = new List<CanvasGroup>();
    private Vector3 target;
    private Vector2 center = new Vector2(0.5f, 0.5f);
    private float time = -1f;
    private float revealTime;
    private float punch;
    private bool covered;
    private bool revealing;
    private bool punched;
    private Camera punchedEye;
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private float savedFov;

    public bool IsPlaying
    {
        get { return time >= 0f; }
    }

    public void Play(Camera eye, Vector3 target, BattleTransitionConfig config)
    {
        if (IsPlaying)
            return;

        this.eye = eye;
        this.target = target;
        this.config = config;
        time = 0f;
        revealTime = 0f;
        punch = 0f;
        covered = false;
        revealing = false;
        Time.timeScale = 0f;
    }

    public void Cancel()
    {
        if (!IsPlaying)
            return;

        Finish();
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginContextRendering += OnBeginContext;
        RenderPipelineManager.endContextRendering += OnEndContext;
        SceneManager.activeSceneChanged += OnSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginContextRendering -= OnBeginContext;
        RenderPipelineManager.endContextRendering -= OnEndContext;
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (IsPlaying)
            Finish();
    }

    private void LateUpdate()
    {
        if (!IsPlaying)
            return;

        float step = Mathf.Min(Time.unscaledDeltaTime, MaxStep);

        if (!covered)
        {
            time += step;
            float rush = Mathf.Clamp01(time / config.zoomSeconds);
            punch = rush * rush;
            Show(punch, 0f);

            if (rush < 1f)
                return;

            covered = true;

            if (Covered != null)
                Covered.Invoke();

            return;
        }

        if (!revealing)
        {
            Show(1f, 0f);
            return;
        }

        if (revealEye == null)
            revealEye = Camera.main;

        revealTime += step;
        float settle = Mathf.Clamp01(revealTime / config.settleSeconds);
        float ease = Mathf.SmoothStep(0f, 1f, settle);
        float flash = Mathf.Clamp01(1f - revealTime / config.flashSeconds) * config.flash;

        revealZoom = 1f - ease;
        Time.timeScale = ease;
        SetHudAlpha(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((settle - config.hudDelay) / (1f - config.hudDelay))));
        Show(1f - ease, flash);

        if (settle < 1f)
            return;

        Finish();
    }

    private void Finish()
    {
        time = -1f;
        eye = null;
        revealEye = null;
        revealZoom = 0f;
        punch = 0f;
        covered = false;
        revealing = false;
        Time.timeScale = 1f;
        SetHudAlpha(1f);
        hudGroups.Clear();
        Show(0f, 0f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsPlaying)
            return;

        hudGroups.Clear();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                if (!canvas.isRootCanvas || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    continue;

                CanvasGroup group = canvas.GetComponent<CanvasGroup>();

                if (group == null)
                    group = canvas.gameObject.AddComponent<CanvasGroup>();

                hudGroups.Add(group);
            }
        }

        SetHudAlpha(0f);
    }

    private void SetHudAlpha(float alpha)
    {
        foreach (CanvasGroup group in hudGroups)
        {
            if (group != null)
                group.alpha = alpha;
        }
    }

    private void OnSceneChanged(Scene from, Scene to)
    {
        if (!IsPlaying || !covered || revealing)
            return;

        revealing = true;
        eye = null;
        revealEye = null;
        revealZoom = 1f;
        punch = 0f;
    }

    private void OnBeginContext(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (revealEye != null && revealZoom > 0f)
        {
            savedPosition = revealEye.transform.position;
            savedRotation = revealEye.transform.rotation;
            savedFov = revealEye.fieldOfView;
            punchedEye = revealEye;
            punched = true;
            revealEye.fieldOfView = Mathf.Lerp(savedFov, config.revealFov, revealZoom);
            return;
        }

        if (eye == null || punch <= 0f)
            return;

        Transform place = eye.transform;
        Vector3 look = target - place.position;

        if (look.sqrMagnitude < 0.0001f)
            return;

        savedPosition = place.position;
        savedRotation = place.rotation;
        savedFov = eye.fieldOfView;
        punchedEye = eye;
        punched = true;
        place.rotation = Quaternion.Slerp(savedRotation, Quaternion.LookRotation(look), punch);
        eye.fieldOfView = Mathf.Lerp(savedFov, config.punchFov, punch);
    }

    private void OnEndContext(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (!punched)
            return;

        punched = false;

        if (punchedEye == null)
            return;

        punchedEye.transform.SetPositionAndRotation(savedPosition, savedRotation);
        punchedEye.fieldOfView = savedFov;
        punchedEye = null;
    }

    private void Show(float blur, float flash)
    {
        if (eye != null)
        {
            Vector3 point = eye.WorldToViewportPoint(target);

            if (point.z > 0f)
                center = Vector2.Lerp(point, new Vector2(0.5f, 0.5f), punch);
        }

        Shader.SetGlobalFloat(BlurId, Mathf.Clamp01(blur));
        Shader.SetGlobalFloat(FlashId, Mathf.Clamp01(flash));
        Shader.SetGlobalVector(CenterId, new Vector4(center.x, center.y, 0f, 0f));
    }
}
