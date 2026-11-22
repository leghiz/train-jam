using UnityEngine;
using UnityEngine.UI;

public class TurnQueueView : MonoBehaviour
{
    [SerializeField] private Image[] slots;

    [SerializeField] private Color playerColor = new Color(0.3f, 0.8f, 0.32f);
    [SerializeField] private Color enemyColor = new Color(0.85f, 0.2f, 0.18f);
    [SerializeField] private float dimBrightness = 0.4f;

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;

        if (turnManager == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i == 0)
            {
                ShowSlot(i, turnManager.CurrentTurn, false);
                continue;
            }

            int queueIndex = i - 1;
            bool hasTurn = queueIndex < turnManager.Queue.Count;
            slots[i].transform.parent.gameObject.SetActive(hasTurn);

            if (hasTurn)
                ShowSlot(i, turnManager.Queue[queueIndex], true);
        }
    }

    private void ShowSlot(int index, TurnManager.Turn turn, bool isDimmed)
    {
        Color color = enemyColor;

        if (turn == TurnManager.Turn.Player)
            color = playerColor;

        if (isDimmed)
            color = new Color(color.r * dimBrightness, color.g * dimBrightness, color.b * dimBrightness, color.a);

        slots[index].color = color;
    }
}
