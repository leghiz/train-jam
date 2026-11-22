using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private GameObject dropPrompt;

    [Header("Exploration")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerCombatTargeting playerTargeting;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private RectTransform cursorRing;
    [SerializeField] private GameObject eventSystem;

    [SerializeField] private string healthColor = "#7FD36B";
    [SerializeField] private string stressColor = "#F2B33D";

    private int selectedIndex = -1;
    private bool wasMovementEnabled;
    private bool wasRotationEnabled;
    private bool wasTargetingEnabled;

    public bool IsOpen
    {
        get { return window.activeSelf; }
    }

    private bool IsBattle
    {
        get { return turnActions != null; }
    }

    private void Awake()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Init(this, i);
        }

        window.SetActive(false);
        details.SetActive(false);

        if (!IsBattle)
            FindPlayer();
    }

    private void FindPlayer()
    {
        if (inventory == null)
            inventory = FindAnyObjectByType<InventorySystem>();

        if (inventory == null)
            return;

        if (playerMovement == null)
            playerMovement = inventory.GetComponent<PlayerMovement>();

        if (playerRotation == null)
            playerRotation = inventory.GetComponent<PlayerRotation>();

        if (playerTargeting == null)
            playerTargeting = inventory.GetComponent<PlayerCombatTargeting>();

        if (playerRigidbody == null)
            playerRigidbody = inventory.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        UpdateEventSystem();
        HandleToggle();

        if (!IsOpen)
            return;

        if (IsBattle && !turnActions.CanUseItem)
        {
            Close();
            return;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Show(GetItem(i));
        }

        weaponLabel.text = "";

        WeaponData weapon = GetWeapon();

        if (weapon != null)
            weaponLabel.text = weapon.displayName.ToUpper();

        MoveCursor();
        RefreshDetails();
    }

    public void Open()
    {
        window.SetActive(true);

        if (!IsBattle)
            LockPlayer();

        Select(0);
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        selectedIndex = -1;
        details.SetActive(false);
        window.SetActive(false);

        if (!IsBattle)
            UnlockPlayer();
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
        ItemData item = GetItem(index);

        if (item == null)
            return;

        if (IsBattle)
        {
            if (turnActions.UseItem(index))
                Close();

            return;
        }

        if (CanUse(item))
            GetInventory().Use(index);
    }

    public void DropSlot(int index)
    {
        ItemData item = GetItem(index);

        if (item == null || !CanDrop(item))
            return;

        GetInventory().Drop(index);
    }

    private void UpdateEventSystem()
    {
        if (eventSystem == null)
            return;

        bool isActive = !BattleContext.HasEncounter;

        if (eventSystem.activeSelf != isActive)
            eventSystem.SetActive(isActive);
    }

    private void HandleToggle()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || !keyboard.tabKey.wasPressedThisFrame)
            return;

        if (IsOpen)
        {
            Close();
            return;
        }

        if (IsBattle || BattleContext.HasEncounter)
            return;

        if (playerTargeting != null && playerTargeting.IsAiming)
            return;

        Open();
    }

    private void LockPlayer()
    {
        wasMovementEnabled = playerMovement != null && playerMovement.enabled;
        wasRotationEnabled = playerRotation != null && playerRotation.enabled;
        wasTargetingEnabled = playerTargeting != null && playerTargeting.enabled;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerRotation != null)
            playerRotation.enabled = false;

        if (playerTargeting != null)
            playerTargeting.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (cursorRing != null)
            Cursor.visible = false;
    }

    private void UnlockPlayer()
    {
        if (playerMovement != null)
            playerMovement.enabled = wasMovementEnabled;

        if (playerRotation != null)
            playerRotation.enabled = wasRotationEnabled;

        if (playerTargeting != null)
            playerTargeting.enabled = wasTargetingEnabled;

        Cursor.visible = true;
    }

    private void MoveCursor()
    {
        Mouse mouse = Mouse.current;

        if (cursorRing == null || mouse == null)
            return;

        cursorRing.position = mouse.position.ReadValue();
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
        usePrompt.SetActive(CanUse(item));

        if (dropPrompt != null)
            dropPrompt.SetActive(CanDrop(item));

        details.SetActive(true);
    }

    private bool CanUse(ItemData item)
    {
        if (IsBattle)
            return true;

        return item.heal > 0f;
    }

    private bool CanDrop(ItemData item)
    {
        if (IsBattle)
            return false;

        return item.worldPrefab != null;
    }

    private InventorySystem GetInventory()
    {
        if (inventory != null)
            return inventory;

        return BattleContext.Inventory;
    }

    private WeaponData GetWeapon()
    {
        if (IsBattle)
            return BattleContext.Weapon;

        InventorySystem current = GetInventory();

        if (current == null)
            return null;

        return current.Weapon;
    }

    private ItemData GetItem(int index)
    {
        InventorySystem current = GetInventory();

        if (current == null || index < 0 || index >= current.Slots.Count)
            return null;

        return current.Slots[index];
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
