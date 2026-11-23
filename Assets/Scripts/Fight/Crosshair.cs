using UnityEngine;
using UnityEngine.InputSystem;
using static TurnManager;

[RequireComponent(typeof(RectTransform))]
public class Crosshair : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private GameObject visual;
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

        visual.SetActive(isPlayerTurn);
        Cursor.visible = false;

        UpdateCrosshair();
        UpdateOscillation();
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
