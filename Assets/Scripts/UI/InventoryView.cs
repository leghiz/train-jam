using TMPro;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private GameObject window;
    [SerializeField] private InventoryCellView[] cells;
    [SerializeField] private TMP_Text weaponLabel;
    [SerializeField] private PlayerTurnActions turnActions;

    [SerializeField] private GameObject details;
    [SerializeField] private TMP_Text detailsTitle;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private GameObject usePrompt;

    [SerializeField] private string healthColor = "#7FD36B";
    [SerializeField] private string stressColor = "#F2B33D";

    private int selectedIndex = -1;

    public bool IsOpen
    {
        get { return window.activeSelf; }
    }

    private void Awake()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Init(this, i);
        }

        window.SetActive(false);
        details.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        if (!turnActions.CanUseItem)
        {
            Close();
            return;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Show(GetItem(i));
        }

        weaponLabel.text = "";

        if (BattleContext.Weapon != null)
            weaponLabel.text = BattleContext.Weapon.displayName.ToUpper();

        RefreshDetails();
    }

    public void Open()
    {
        window.SetActive(true);
        Select(0);
    }

    public void Close()
    {
        selectedIndex = -1;
        details.SetActive(false);
        window.SetActive(false);
    }

    public void Select(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].SetSelected(i == index);
        }

        RefreshDetails();
    }

    public void UseSlot(int index)
    {
        if (turnActions.UseItem(index))
            Close();
    }

    private void RefreshDetails()
    {
        ItemData item = GetItem(selectedIndex);

        if (item == null)
        {
            details.SetActive(false);
            return;
        }

        detailsTitle.text = item.displayName.ToUpper();
        detailsText.text = GetDescription(item);
        usePrompt.SetActive(true);
        details.SetActive(true);
    }

    private ItemData GetItem(int index)
    {
        InventorySystem inventory = BattleContext.Inventory;

        if (inventory == null || index < 0 || index >= inventory.Slots.Count)
            return null;

        return inventory.Slots[index];
    }

    private string GetDescription(ItemData item)
    {
        if (!string.IsNullOrEmpty(item.description))
            return item.description;

        string text = "";

        if (item.heal > 0f)
            text += "Восстанавливает <color=" + healthColor + ">здоровье</color>. ";

        if (item.stressChange < 0f)
            text += "Снимает <color=" + stressColor + ">стресс</color>.";

        return text.Trim();
    }
}
