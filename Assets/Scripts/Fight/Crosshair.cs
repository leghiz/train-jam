using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static TurnManager;

[RequireComponent(typeof(RectTransform))]
public class Crosshair : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private GameObject visual;
    [SerializeField] private RectTransform cursor;

    [Header("Reload")]
    [SerializeField] private Shooting shooting;
    [SerializeField] private Image reloadRing;

    [Header("Kick")]
    [SerializeField] private RectTransform[] ticks;
    [SerializeField] private float tickDistance = 24f;
    [SerializeField] private float kickSpread = 14f;
    [SerializeField] private float kickRecoil = 45f;
    [SerializeField] private float kickRecovery = 6f;

    private float spread;
    [SerializeField] private float aimingSpeed = 5f;

    [Header("Oscillation")]
    [SerializeField] private float shakeSpeed = 2f;
    [SerializeField] private float blendSpeed = 5f;

    private RectTransform rectTransform;

    private float targetAmplitude;
    private float currentAmplitude;
    private float time;

    private bool shakePaused;
    private int turnCount;

    private Vector3 basePosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        WeaponData weapon = BattleContext.Weapon;

        if (weapon != null)
        {
            aimingSpeed = weapon.aimingSpeed;
            targetAmplitude = weapon.aimingOscillation;
        }

        basePosition = rectTransform.position;
    }

    private void Start()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnChanged += HandleTurnChanged;
    }

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;

        bool isPlayerTurn = turnManager != null &&
                            turnManager.CurrentTurn == TurnManager.Turn.Player;

        bool isAiming = isPlayerTurn && turnManager.AttackChosen;

        visual.SetActive(isAiming);
        Cursor.visible = false;

        UpdateCursor();

        if (!isAiming)
        {
            SnapToMouse();
            return;
        }

        UpdateCrosshair();
        UpdateOscillation();
        UpdateReloadRing();
        UpdateTicks();
    }

    public void Kick()
    {
        spread = 1f;
        basePosition += Vector3.up * kickRecoil;
        rectTransform.position += Vector3.up * kickRecoil;
    }

    private void UpdateReloadRing()
    {
        if (reloadRing == null || shooting == null)
            return;

        float progress = shooting.ReloadProgress;

        reloadRing.enabled = progress < 1f;
        reloadRing.fillAmount = progress;
    }

    private void UpdateTicks()
    {
        spread = Mathf.MoveTowards(spread, 0f, kickRecovery * Time.deltaTime);

        if (ticks == null)
            return;

        float distance = tickDistance + kickSpread * spread * spread;

        for (int i = 0; i < ticks.Length; i++)
        {
            if (ticks[i] == null)
                continue;

            Vector2 direction = ticks[i].anchoredPosition.normalized;
            ticks[i].anchoredPosition = direction * distance;
        }
    }

    private void UpdateCursor()
    {
        Mouse mouse = Mouse.current;

        if (cursor == null || mouse == null)
            return;

        cursor.position = mouse.position.ReadValue();
    }

    private void SnapToMouse()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        rectTransform.position = mouse.position.ReadValue();
        basePosition = rectTransform.position;
    }

    private void UpdateCrosshair()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector3 targetPosition = mouse.position.ReadValue();

        Vector3 offset = rectTransform.position - basePosition;

        rectTransform.position = Vector3.Lerp(
            rectTransform.position - offset,
            targetPosition,
            aimingSpeed * Time.deltaTime
        );

        basePosition = rectTransform.position;
    }

    private void UpdateOscillation()
    {
        if (shakePaused)
        {
            currentAmplitude = 0f;
            return;
        }

        float t = 1f - Mathf.Exp(-blendSpeed * Time.deltaTime);

        currentAmplitude = Mathf.Lerp(
            currentAmplitude,
            targetAmplitude,
            t
        );

        time += Time.deltaTime * shakeSpeed;

        float nx = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
        float ny = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f;

        Vector3 shakeOffset = new Vector3(
            nx * currentAmplitude,
            ny * currentAmplitude,
            0f
        );

        rectTransform.position += shakeOffset;
    }

    public void SetWeapon(WeaponData weapon)
    {
        if (weapon == null)
            return;

        aimingSpeed = weapon.aimingSpeed;
        targetAmplitude = weapon.aimingOscillation;
    }

    public void PauseShake()
    {
        shakePaused = true;
        turnCount = 0;
    }

    public void ResumeShake()
    {
        shakePaused = false;
        Debug.Log("тряска");
    }

    private void HandleTurnChanged(TurnManager.Turn turn)
    {
        if (turn == Turn.Player)
        {
            turnCount++;

            if (turnCount == 2)
            {
                ResumeShake();
                turnCount = 0;
            }
        }
    }

    private void OnDisable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnChanged -= HandleTurnChanged;

        Cursor.visible = true;
    }
}
