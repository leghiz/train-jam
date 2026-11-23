using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class Crosshair : MonoBehaviour
{
    [SerializeField] private GameObject visual;

    private RectTransform rectTransform;
    private float aimingSpeed = 5f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        WeaponData weapon = BattleContext.Weapon;

        if (weapon != null)
            aimingSpeed = weapon.aimingSpeed;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;
        bool isPlayerTurn = turnManager != null &&
                            turnManager.CurrentTurn == TurnManager.Turn.Player;

        visual.SetActive(isPlayerTurn);
        Cursor.visible = false;

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector3 targetPosition = mouse.position.ReadValue();

        rectTransform.position = Vector3.Lerp(
            rectTransform.position,
            targetPosition,
            aimingSpeed * Time.deltaTime
        );
    }
}
