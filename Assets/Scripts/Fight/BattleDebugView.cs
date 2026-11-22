using UnityEngine;

public class BattleDebugView : MonoBehaviour
{
    private Color armorPiercingColor = new Color(0.15f, 0.3f, 0.95f);
    private Color normalColor = new Color(0.9f, 0.1f, 0.1f);

    private Ammo ammo;
    private PlayerTurnActions turnActions;
    private GUIStyle labelStyle;
    private float scale;

    private void Start()
    {
        ammo = FindAnyObjectByType<Ammo>();
        turnActions = GetComponent<PlayerTurnActions>();
    }

    private void OnGUI()
    {
        scale = Screen.height / 1080f;

        if (labelStyle == null)
            labelStyle = new GUIStyle(GUI.skin.label);

        labelStyle.fontSize = Mathf.RoundToInt(28f * scale);
        labelStyle.fontStyle = FontStyle.Bold;

        DrawTurn();
        DrawAmmo();
        DrawInventory();
    }

    private void DrawTurn()
    {
        TurnManager turnManager = TurnManager.Instance;

        if (turnManager == null)
            return;

        string text = "Ход врага";

        if (turnManager.CurrentTurn == TurnManager.Turn.Player && !turnManager.AttackChosen)
            text = "Выберите: ЛКМ — атака, 1–4 — предмет";

        if (turnManager.CurrentTurn == TurnManager.Turn.Player && turnManager.AttackChosen)
            text = "Атака: " + turnManager.TimeLeft.ToString("0.0") + " с";

        DrawLabel(new Rect(Screen.width - 460f * scale, Screen.height - 250f * scale, 430f * scale, 40f * scale), text, Color.white);
    }

    private void DrawAmmo()
    {
        if (ammo == null)
            return;

        float size = 70f * scale;
        float spacing = 14f * scale;
        float right = Screen.width - 30f * scale;
        float top = Screen.height - 30f * scale - size;

        string text = "Патроны " + ammo.RoundsLeft + "/" + ammo.MagazineSize;
        Color textColor = Color.white;

        if (ammo.IsReloading)
        {
            text = "ПЕРЕЗАРЯДКА " + ammo.ReloadTimeLeft.ToString("0.0") + " с";
            textColor = Color.yellow;
        }

        DrawLabel(new Rect(Screen.width - 460f * scale, top - 50f * scale, 430f * scale, 40f * scale), text, textColor);

        for (int i = 0; i < ammo.Queue.Count; i++)
        {
            float left = right - (ammo.Queue.Count - i) * (size + spacing) + spacing;
            Rect rect = new Rect(left, top, size, size);

            if (i == 0)
                DrawRect(new Rect(rect.x - 4f * scale, rect.y - 4f * scale, rect.width + 8f * scale, rect.height + 8f * scale), Color.white);

            Color color = normalColor;

            if (ammo.Queue[i] == "blue")
                color = armorPiercingColor;

            if (ammo.IsReloading)
                color = Color.Lerp(color, Color.black, 0.6f);

            DrawRect(rect, color);
        }
    }

    private void DrawInventory()
    {
        InventorySystem inventory = BattleContext.Inventory;

        if (inventory == null)
            return;

        bool canUse = turnActions != null && turnActions.CanUseItem;
        string status = "можно использовать";

        if (!canUse)
            status = GetBlockedReason();

        float left = 30f * scale;
        float lineHeight = 40f * scale;
        float top = Screen.height - 30f * scale - lineHeight * (inventory.Slots.Count + 1);

        Color textColor = Color.white;

        if (!canUse)
            textColor = new Color(1f, 1f, 1f, 0.4f);

        DrawLabel(new Rect(left, top, 700f * scale, lineHeight), "Инвентарь: " + status, textColor);

        for (int i = 0; i < inventory.Slots.Count; i++)
        {
            string itemName = "пусто";

            if (inventory.Slots[i] != null)
                itemName = inventory.Slots[i].displayName;

            Rect rect = new Rect(left, top + lineHeight * (i + 1), 700f * scale, lineHeight);
            DrawLabel(rect, (i + 1) + " — " + itemName, textColor);
        }
    }

    private string GetBlockedReason()
    {
        TurnManager turnManager = TurnManager.Instance;

        if (turnManager == null || turnManager.CurrentTurn != TurnManager.Turn.Player)
            return "ход врага";

        if (turnManager.AttackChosen)
            return "выбрана атака";

        return "нельзя";
    }

    private void DrawLabel(Rect rect, string text, Color color)
    {
        Color oldColor = GUI.color;

        GUI.color = new Color(0f, 0f, 0f, color.a);
        GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), text, labelStyle);

        GUI.color = color;
        GUI.Label(rect, text, labelStyle);

        GUI.color = oldColor;
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = oldColor;
    }
}
