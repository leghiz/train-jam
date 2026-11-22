using TMPro;
using UnityEngine;

public class PickupPromptView : MonoBehaviour
{
    [SerializeField] private PlayerInteraction interaction;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private InventoryView inventoryView;
    [SerializeField] private RectTransform prompt;
    [SerializeField] private GameObject keyPlate;
    [SerializeField] private GameObject wordPlate;
    [SerializeField] private TMP_Text wordLabel;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private string fullText = "НЕТ МЕСТА";
    [SerializeField] private string enemyText = "РЯДОМ ВРАГ";
    [SerializeField] private string lockedText = "НУЖЕН КЛЮЧ";
    [SerializeField] private NoteView noteView;
    [SerializeField] private EndingView endingView;
    [SerializeField] private float offset = 5f;
    [SerializeField] private float arrowGap = 4f;

    private RectTransform area;

    private void Awake()
    {
        area = transform as RectTransform;
        prompt.gameObject.SetActive(false);
        arrow.gameObject.SetActive(false);

        if (interaction == null)
            interaction = FindAnyObjectByType<PlayerInteraction>();

        if (inventory == null && interaction != null)
            inventory = interaction.GetComponent<InventorySystem>();

        if (noteView == null)
            noteView = FindAnyObjectByType<NoteView>();

        if (endingView == null)
            endingView = FindAnyObjectByType<EndingView>();
    }

    private void LateUpdate()
    {
        GameObject target = null;
        string blockedText = "";

        if (CanShow())
        {
            ItemPickup pickup = interaction.FindNearestPickup();
            NoteInteraction note = FindNote();
            DoorInteraction door = FindDoor();
            StoveInteraction stove = FindStove();

            if (pickup != null)
            {
                target = pickup.gameObject;

                if (!HasRoom(pickup.Item))
                    blockedText = fullText;
            }
            else if (note != null)
            {
                target = note.Visual;

                if (target == null)
                    target = note.gameObject;
            }
            else if (door != null)
            {
                target = door.gameObject;
                blockedText = GetDoorBlockedText(door);
            }
            else if (stove != null)
            {
                target = stove.gameObject;

                if (stove.HasEnemiesLeft)
                    blockedText = enemyText;
            }
        }

        Camera cam = Camera.main;
        Rect screenRect = new Rect();
        bool isVisible = target != null && cam != null && TryGetScreenRect(cam, target, out screenRect);
        bool canInteract = blockedText == "";

        prompt.gameObject.SetActive(isVisible);
        arrow.gameObject.SetActive(isVisible && canInteract);

        if (!isVisible)
            return;

        keyPlate.SetActive(canInteract);
        wordPlate.SetActive(!canInteract);
        wordLabel.text = blockedText;

        Vector2 top = ToLocal(new Vector2(screenRect.center.x, screenRect.yMax)) + new Vector2(0f, offset);

        if (canInteract)
        {
            arrow.anchoredPosition = top;
            top += new Vector2(0f, arrow.rect.height + arrowGap);
        }

        prompt.anchoredPosition = top;
    }

    private bool CanShow()
    {
        if (BattleContext.HasEncounter)
            return false;

        if (inventoryView != null && inventoryView.IsOpen)
            return false;

        if (noteView != null && noteView.IsOpen)
            return false;

        if (endingView != null && endingView.IsShown)
            return false;

        return interaction != null && interaction.isActiveAndEnabled;
    }

    private NoteInteraction FindNote()
    {
        NoteInteraction[] notes = FindObjectsByType<NoteInteraction>();

        foreach (NoteInteraction note in notes)
        {
            if (note.PlayerNearby)
                return note;
        }

        return null;
    }

    private StoveInteraction FindStove()
    {
        StoveInteraction[] stoves = FindObjectsByType<StoveInteraction>();

        foreach (StoveInteraction stove in stoves)
        {
            if (stove.PlayerNearby)
                return stove;
        }

        return null;
    }

    private DoorInteraction FindDoor()
    {
        DoorInteraction[] doors = FindObjectsByType<DoorInteraction>();

        foreach (DoorInteraction door in doors)
        {
            if (door.PlayerNearby)
                return door;
        }

        return null;
    }

    private string GetDoorBlockedText(DoorInteraction door)
    {
        if (door.HasEnemiesLeft)
            return enemyText;

        if (door.IsLocked)
            return lockedText;

        return "";
    }

    private bool HasRoom(PickupData item)
    {
        ItemData data = item as ItemData;

        if (data == null || data.isKey || inventory == null)
            return true;

        foreach (ItemData slot in inventory.Slots)
        {
            if (slot == null)
                return true;
        }

        return false;
    }

    private bool TryGetScreenRect(Camera cam, GameObject target, out Rect screenRect)
    {
        screenRect = new Rect();
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return false;

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? bounds.min.x : bounds.max.x,
                (i & 2) == 0 ? bounds.min.y : bounds.max.y,
                (i & 4) == 0 ? bounds.min.z : bounds.max.z);

            Vector3 point = cam.WorldToScreenPoint(corner);

            if (point.z <= 0f)
                return false;

            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }

        screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }

    private Vector2 ToLocal(Vector2 screenPoint)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPoint, null, out local);
        return local;
    }
}
