using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class Crosshair : MonoBehaviour
{
    [SerializeField] private GameObject visual;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;
        bool isPlayerTurn = turnManager != null && turnManager.CurrentTurn == TurnManager.Turn.Player;

        visual.SetActive(isPlayerTurn);
        Cursor.visible = false;

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        rectTransform.position = mouse.position.ReadValue();
    }
}
