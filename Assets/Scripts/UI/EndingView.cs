using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndingView : MonoBehaviour
{
    [SerializeField] private GameObject screen;
    [SerializeField] private Image fade;
    [SerializeField] private CanvasGroup content;
    [SerializeField] private float fadeTime = 1.5f;
    [SerializeField] private float contentFadeTime = 0.8f;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerCombatTargeting playerTargeting;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private GameRestart gameRestart;

    private bool isShown;
    private float timer;

    public bool IsShown => isShown;

    private void Awake()
    {
        FindPlayer();
        screen.SetActive(false);
    }

    private void Update()
    {
        if (!isShown)
            return;

        timer += Time.unscaledDeltaTime;

        SetFade(Mathf.Clamp01(timer / fadeTime));
        content.alpha = Mathf.Clamp01((timer - fadeTime) / contentFadeTime);

        if (timer < fadeTime + contentFadeTime)
            return;

        if (IsAnyPressed())
            StartNewRun();
    }

    public void Show()
    {
        if (isShown)
            return;

        isShown = true;
        timer = 0f;
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        screen.SetActive(true);
        SetFade(0f);
        content.alpha = 0f;
        LockPlayer();
    }

    private void StartNewRun()
    {
        isShown = false;
        screen.SetActive(false);

        if (playerRotation != null)
            playerRotation.enabled = true;

        if (playerTargeting != null)
            playerTargeting.enabled = true;

        if (gameRestart != null)
            gameRestart.Restart();
        else if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private void SetFade(float alpha)
    {
        Color color = fade.color;
        color.a = alpha;
        fade.color = color;
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

    private void FindPlayer()
    {
        InventorySystem inventory = FindAnyObjectByType<InventorySystem>();

        if (inventory == null)
            return;

        if (playerMovement == null)
            playerMovement = inventory.GetComponent<PlayerMovement>();

        if (playerRotation == null)
            playerRotation = inventory.GetComponent<PlayerRotation>();

        if (playerTargeting == null)
            playerTargeting = inventory.GetComponent<PlayerCombatTargeting>();

        if (playerRigidbody == null)
            playerRigidbody = inventory.GetComponent<Rigidbody>();

        if (gameRestart == null)
            gameRestart = inventory.GetComponent<GameRestart>();
    }

    private void LockPlayer()
    {
        if (playerTargeting != null && playerTargeting.IsAiming)
            playerTargeting.StopAiming();

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerRotation != null)
            playerRotation.enabled = false;

        if (playerTargeting != null)
            playerTargeting.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }
}
