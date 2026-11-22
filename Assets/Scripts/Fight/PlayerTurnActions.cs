using UnityEngine;

public class PlayerTurnActions : MonoBehaviour
{
    private PlayerStats battlePlayerStats;
    private InventoryInput explorationInput;
    private bool wasExplorationInputEnabled;

    public bool CanUseItem
    {
        get
        {
            TurnManager turnManager = TurnManager.Instance;

            if (turnManager == null || BattleContext.Inventory == null)
                return false;

            return turnManager.CurrentTurn == TurnManager.Turn.Player && !turnManager.AttackChosen;
        }
    }

    public void ChooseAttack()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.ChooseAttack();
    }

    private void OnEnable()
    {
        if (BattleContext.Inventory == null)
            return;

        explorationInput = BattleContext.Inventory.GetComponent<InventoryInput>();

        if (explorationInput != null)
        {
            wasExplorationInputEnabled = explorationInput.enabled;
            explorationInput.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (explorationInput != null)
            explorationInput.enabled = wasExplorationInputEnabled;
    }

    public bool UseItem(int index)
    {
        if (!CanUseItem)
            return false;

        if (battlePlayerStats == null)
            battlePlayerStats = FindBattlePlayer();

        if (battlePlayerStats == null)
            return false;

        if (!BattleContext.Inventory.Use(index, battlePlayerStats))
            return false;

        TurnManager.Instance.EndPlayerTurn();
        return true;
    }

    private PlayerStats FindBattlePlayer()
    {
        PlayerStats[] players = FindObjectsByType<PlayerStats>();

        foreach (PlayerStats player in players)
        {
            if (player.gameObject.scene == gameObject.scene)
                return player;
        }

        return null;
    }
}
