using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackHudView : MonoBehaviour
{
    [SerializeField] private GameObject group;
    [SerializeField] private Image timerFill;
    [SerializeField] private Image[] ammoSlots;
    [SerializeField] private Ammo ammo;

    [SerializeField] private Color armorPiercingColor = new Color(0.2f, 0.35f, 0.95f);
    [SerializeField] private Color normalColor = new Color(0.85f, 0.12f, 0.1f);
    [SerializeField] private float reloadDimming = 0.6f;

    [SerializeField] private float slideTime = 0.12f;
    [SerializeField] private Vector2 flyVelocity = new Vector2(-260f, 620f);
    [SerializeField] private float flyGravity = 2400f;
    [SerializeField] private float flySpin = 720f;
    [SerializeField] private float flyLifetime = 1.5f;

    private class FlyingBullet
    {
        public RectTransform rect;
        public Vector2 velocity;
        public float age;
    }

    private RectTransform[] slotRects;
    private Vector2[] basePositions;
    private Vector2[] baseSizes;
    private float slideProgress = 1f;
    private List<FlyingBullet> flying = new List<FlyingBullet>();

    private void Awake()
    {
        slotRects = new RectTransform[ammoSlots.Length];
        basePositions = new Vector2[ammoSlots.Length];
        baseSizes = new Vector2[ammoSlots.Length];

        for (int i = 0; i < ammoSlots.Length; i++)
        {
            slotRects[i] = ammoSlots[i].transform.parent as RectTransform;
            basePositions[i] = slotRects[i].anchoredPosition;
            baseSizes[i] = slotRects[i].sizeDelta;
        }

        if (ammo != null)
            ammo.Fired += OnFired;
    }

    private void OnDestroy()
    {
        if (ammo != null)
            ammo.Fired -= OnFired;
    }

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;
        bool isAttacking = turnManager != null && turnManager.CurrentTurn == TurnManager.Turn.Player && turnManager.AttackChosen;

        group.SetActive(isAttacking);

        if (!isAttacking)
            return;

        if (turnManager.config != null && turnManager.config.playerTurnDuration > 0f)
            timerFill.fillAmount = Mathf.Clamp01(turnManager.TimeLeft / turnManager.config.playerTurnDuration);

        UpdateColors();
        UpdateSlide();
        UpdateFlying();
    }

    private void UpdateColors()
    {
        for (int i = 0; i < ammoSlots.Length; i++)
        {
            bool hasBullet = ammo != null && i < ammo.Queue.Count;
            slotRects[i].gameObject.SetActive(hasBullet);

            if (!hasBullet)
                continue;

            Color color = GetColor(ammo.Queue[i]);

            if (ammo.IsReloading)
                color = Color.Lerp(color, Color.black, reloadDimming);

            ammoSlots[i].color = color;
        }
    }

    private void UpdateSlide()
    {
        slideProgress = Mathf.Min(1f, slideProgress + Time.deltaTime / slideTime);
        float t = 1f - (1f - slideProgress) * (1f - slideProgress);
        int last = slotRects.Length - 1;

        for (int i = 0; i < slotRects.Length; i++)
        {
            Vector2 fromPosition = basePositions[last] + (basePositions[last] - basePositions[Mathf.Max(0, last - 1)]);
            Vector2 fromSize = baseSizes[last];

            if (i < last)
            {
                fromPosition = basePositions[i + 1];
                fromSize = baseSizes[i + 1];
            }

            slotRects[i].anchoredPosition = Vector2.Lerp(fromPosition, basePositions[i], t);
            slotRects[i].sizeDelta = Vector2.Lerp(fromSize, baseSizes[i], t);
        }
    }

    private void UpdateFlying()
    {
        for (int i = flying.Count - 1; i >= 0; i--)
        {
            FlyingBullet bullet = flying[i];
            bullet.age += Time.deltaTime;
            bullet.velocity.y -= flyGravity * Time.deltaTime;
            bullet.rect.anchoredPosition += bullet.velocity * Time.deltaTime;
            bullet.rect.Rotate(0f, 0f, flySpin * Time.deltaTime);

            if (bullet.age >= flyLifetime)
            {
                Destroy(bullet.rect.gameObject);
                flying.RemoveAt(i);
            }
        }
    }

    private void OnFired(string type)
    {
        GameObject copy = Instantiate(slotRects[0].gameObject, slotRects[0].parent);
        RectTransform copyRect = copy.GetComponent<RectTransform>();
        copyRect.anchoredPosition = basePositions[0];
        copyRect.sizeDelta = baseSizes[0];
        copy.transform.Find("Fill").GetComponent<Image>().color = GetColor(type);

        FlyingBullet bullet = new FlyingBullet();
        bullet.rect = copyRect;
        bullet.velocity = flyVelocity;
        flying.Add(bullet);

        slideProgress = 0f;
    }

    private Color GetColor(string type)
    {
        if (type == "blue")
            return armorPiercingColor;

        return normalColor;
    }
}
