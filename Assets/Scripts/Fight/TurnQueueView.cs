using UnityEngine;
using UnityEngine.UI;

public class TurnQueueView : MonoBehaviour
{
    [SerializeField] private Image[] slots;

    [SerializeField] private Color playerColor = new Color(0.3f, 0.8f, 0.32f);
    [SerializeField] private Color enemyColor = new Color(0.85f, 0.2f, 0.18f);
    [SerializeField] private float dimBrightness = 0.4f;
    [SerializeField] private float slideTime = 0.25f;

    private RectTransform area;
    private RectTransform[] frames;
    private Image[] frameImages;
    private Vector2 basePosition;
    private Vector2 currentSize;
    private Vector2 nextSize;
    private float step;
    private int lastTurnNumber = -1;
    private float slideProgress = 1f;
    private RectTransform ghost;
    private CanvasGroup ghostGroup;
    private Vector2 ghostStart;

    private void Awake()
    {
        area = transform as RectTransform;
        basePosition = area.anchoredPosition;

        frames = new RectTransform[slots.Length];
        frameImages = new Image[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            frames[i] = slots[i].transform.parent as RectTransform;
            frameImages[i] = frames[i].GetComponent<Image>();
        }

        currentSize = frames[0].sizeDelta;
        nextSize = frames[1].sizeDelta;
        step = currentSize.x;

        HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();

        if (layout != null)
            step += layout.spacing;
    }

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;

        if (turnManager == null)
            return;

        if (turnManager.TurnNumber != lastTurnNumber)
        {
            if (lastTurnNumber >= 0)
                StartSlide();

            lastTurnNumber = turnManager.TurnNumber;
        }

        slideProgress = Mathf.Min(1f, slideProgress + Time.deltaTime / slideTime);
        float t = 1f - (1f - slideProgress) * (1f - slideProgress);

        area.anchoredPosition = basePosition + new Vector2(step * (1f - t), 0f);
        frames[0].sizeDelta = Vector2.Lerp(nextSize, currentSize, t);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i == 0)
            {
                ShowSlot(i, turnManager.CurrentTurn, Mathf.Lerp(dimBrightness, 1f, t), 1f);
                continue;
            }

            int queueIndex = i - 1;
            bool hasTurn = queueIndex < turnManager.Queue.Count;
            frames[i].gameObject.SetActive(hasTurn);

            if (!hasTurn)
                continue;

            float alpha = 1f;

            if (i == slots.Length - 1)
                alpha = t;

            ShowSlot(i, turnManager.Queue[queueIndex], dimBrightness, alpha);
        }

        UpdateGhost(t);
    }

    private void StartSlide()
    {
        if (ghost != null)
            Destroy(ghost.gameObject);

        GameObject copy = Instantiate(frames[0].gameObject, area.parent);
        ghost = copy.GetComponent<RectTransform>();
        ghost.position = frames[0].position;
        ghost.SetSiblingIndex(area.GetSiblingIndex() + 1);
        ghostStart = ghost.anchoredPosition;
        ghostGroup = copy.AddComponent<CanvasGroup>();
        ghostGroup.blocksRaycasts = false;

        slideProgress = 0f;
    }

    private void UpdateGhost(float t)
    {
        if (ghost == null)
            return;

        ghost.anchoredPosition = ghostStart - new Vector2(step * t, 0f);
        ghostGroup.alpha = 1f - t;

        if (t >= 1f)
        {
            Destroy(ghost.gameObject);
            ghost = null;
        }
    }

    private void ShowSlot(int index, TurnManager.Turn turn, float brightness, float alpha)
    {
        Color color = enemyColor;

        if (turn == TurnManager.Turn.Player)
            color = playerColor;

        slots[index].color = new Color(color.r * brightness, color.g * brightness, color.b * brightness, alpha);

        Color frameColor = frameImages[index].color;
        frameColor.a = alpha;
        frameImages[index].color = frameColor;
    }
}
