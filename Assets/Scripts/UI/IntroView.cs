using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroView : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage screen;
    [SerializeField] private AspectRatioFitter fitter;
    [SerializeField] private string fileName = "Intro.mp4";
    [SerializeField] private float skipDelay = 0.3f;

    private RenderTexture texture;
    private List<Behaviour> blocked = new List<Behaviour>();
    private bool isPlaying;
    private float timer;

    private void Awake()
    {
        isPlaying = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        screen.enabled = false;
        BlockInput();

        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, fileName);
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnFinished;
        videoPlayer.errorReceived += OnError;
        videoPlayer.Prepare();
    }

    private void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnFinished;
        videoPlayer.errorReceived -= OnError;

        if (isPlaying)
            Restore();

        if (texture != null)
            texture.Release();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        timer += Time.unscaledDeltaTime;

        if (timer < skipDelay)
            return;

        if (IsAnyPressed())
            Finish();
    }

    private void OnPrepared(VideoPlayer source)
    {
        if (!isPlaying)
            return;

        int width = Mathf.Max(1, (int)source.width);
        int height = Mathf.Max(1, (int)source.height);

        texture = new RenderTexture(width, height, 0);
        source.targetTexture = texture;
        screen.texture = texture;
        screen.enabled = true;

        if (fitter != null)
            fitter.aspectRatio = (float)width / height;

        source.Play();
    }

    private void OnFinished(VideoPlayer source)
    {
        Finish();
    }

    private void OnError(VideoPlayer source, string message)
    {
        Finish();
    }

    private void Finish()
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        videoPlayer.Stop();
        Restore();
        gameObject.SetActive(false);
    }

    private void Restore()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        foreach (Behaviour behaviour in blocked)
        {
            if (behaviour != null)
                behaviour.enabled = true;
        }

        blocked.Clear();
    }

    private void BlockInput()
    {
        foreach (InventoryView view in FindObjectsByType<InventoryView>())
            Block(view);

        foreach (PlayerCombatTargeting targeting in FindObjectsByType<PlayerCombatTargeting>())
            Block(targeting);

        foreach (PlayerInteraction interaction in FindObjectsByType<PlayerInteraction>())
            Block(interaction);
    }

    private void Block(Behaviour behaviour)
    {
        if (!behaviour.enabled)
            return;

        behaviour.enabled = false;
        blocked.Add(behaviour);
    }

    private bool IsAnyPressed()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            return true;

        if (mouse == null)
            return false;

        return mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame;
    }
}
