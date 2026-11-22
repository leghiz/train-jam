using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenuView : MonoBehaviour
{
    [SerializeField] private GameObject group;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private TMP_Text inventoryLabel;
    [SerializeField] private InventoryView inventoryView;
    [SerializeField] private PlayerTurnActions turnActions;

    [SerializeField] private string inventoryText = "ИНВЕНТАРЬ";
    [SerializeField] private string closeText = "ЗАКРЫТЬ";

    private void Awake()
    {
        attackButton.onClick.AddListener(OnAttackClicked);
        inventoryButton.onClick.AddListener(OnInventoryClicked);
    }

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;
        bool isChoosing = turnManager != null && turnManager.CurrentTurn == TurnManager.Turn.Player && !turnManager.AttackChosen;

        group.SetActive(isChoosing);

        if (!isChoosing)
            return;

        attackButton.interactable = !inventoryView.IsOpen;

        if (inventoryView.IsOpen)
            inventoryLabel.text = closeText;
        else
            inventoryLabel.text = inventoryText;
    }

    private void OnAttackClicked()
    {
        turnActions.ChooseAttack();
    }

    private void OnInventoryClicked()
    {
        if (inventoryView.IsOpen)
            inventoryView.Close();
        else
            inventoryView.Open();
    }
}
