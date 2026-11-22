using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image plate;
    [SerializeField] private TMP_Text label;

    [SerializeField] private Color plateColor = new Color(0.35f, 0.38f, 0.4f);
    [SerializeField] private Color hoverPlateColor = new Color(1f, 1f, 0.98f);
    [SerializeField] private Color textColor = new Color(0.95f, 0.94f, 0.9f, 0.9f);
    [SerializeField] private Color hoverTextColor = new Color(0.07f, 0.07f, 0.08f);
    [SerializeField] private Color disabledTextColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private Button button;
    private bool isHovered;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnDisable()
    {
        isHovered = false;
    }

    private void Update()
    {
        if (!button.interactable)
        {
            plate.color = plateColor;
            label.color = disabledTextColor;
            return;
        }

        if (isHovered)
        {
            plate.color = hoverPlateColor;
            label.color = hoverTextColor;
        }
        else
        {
            plate.color = plateColor;
            label.color = textColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
