using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCellView : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private Image highlight;
    [SerializeField] private RectTransform hoverFrame;
    [SerializeField] private TMP_Text label;

    [SerializeField] private float highlightFadeTime = 0.08f;
    [SerializeField] private float frameGrow = 6f;
    [SerializeField] private float framePop = 8f;
    [SerializeField] private float springStiffness = 600f;
    [SerializeField] private float springDamping = 22f;

    private InventoryView owner;
    private int index;
    private bool isSelected;
    private float highlightAlpha;
    private float frameOffset;
    private float frameVelocity;

    public void Init(InventoryView inventoryView, int slotIndex)
    {
        owner = inventoryView;
        index = slotIndex;
    }

    public void Show(ItemData item)
    {
        label.text = "";

        if (item != null)
            label.text = item.displayName.ToUpper();
    }

    public void SetSelected(bool selected)
    {
        if (selected && !isSelected)
        {
            frameOffset = framePop;
            frameVelocity = 0f;
        }

        isSelected = selected;
        hoverFrame.gameObject.SetActive(selected);
    }

    private void OnDisable()
    {
        isSelected = false;
        highlightAlpha = 0f;
        ApplyHighlight();
        hoverFrame.gameObject.SetActive(false);
    }

    private void Update()
    {
        float target = 0f;

        if (isSelected)
            target = 1f;

        highlightAlpha = Mathf.MoveTowards(highlightAlpha, target, Time.unscaledDeltaTime / highlightFadeTime);
        ApplyHighlight();

        if (!isSelected)
            return;

        float acceleration = -springStiffness * frameOffset - springDamping * frameVelocity;
        frameVelocity += acceleration * Time.unscaledDeltaTime;
        frameOffset += frameVelocity * Time.unscaledDeltaTime;

        float grow = (frameGrow + frameOffset) * 2f;
        hoverFrame.offsetMin = new Vector2(-grow * 0.5f, -grow * 0.5f);
        hoverFrame.offsetMax = new Vector2(grow * 0.5f, grow * 0.5f);
    }

    private void ApplyHighlight()
    {
        Color color = highlight.color;
        color.a = highlightAlpha;
        highlight.color = color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        owner.Select(index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            owner.UseSlot(index);
    }
}
