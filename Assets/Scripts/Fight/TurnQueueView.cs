using UnityEngine;
using UnityEngine.UI;

public class TurnQueueView : MonoBehaviour
{
    [SerializeField] private Image[] slots;
    [SerializeField] private Text[] labels;

    [SerializeField] private Color playerColor = new Color(0.18f, 0.55f, 0.1f);
    [SerializeField] private Color enemyColor = new Color(0.9f, 0.05f, 0.05f);
    [SerializeField] private float dimming = 0.45f;

    [SerializeField] private string playerText = "ИГРОК";
    [SerializeField] private string enemyText = "ВРАГ";

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
            slots[i].gameObject.SetActive(hasTurn);

            if (hasTurn)
                ShowSlot(i, turnManager.Queue[queueIndex], true);
        }
    }

    private void ShowSlot(int index, TurnManager.Turn turn, bool isDimmed)
    {
        Color color = enemyColor;
        string text = enemyText;

        if (turn == TurnManager.Turn.Player)
        {
            color = playerColor;
            text = playerText;
        }

        if (isDimmed)
            color = Color.Lerp(color, Color.black, dimming);

        slots[index].color = color;
        labels[index].text = text;
    }
}
