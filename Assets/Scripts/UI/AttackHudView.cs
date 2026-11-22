using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AttackHudView : MonoBehaviour
{
    [SerializeField] private GameObject group;
    [SerializeField] private Image timerFill;
    [SerializeField] private RectTransform[] ammoMarkers;
    [SerializeField] private Ammo ammo;

    [SerializeField] private GameObject normalBulletPrefab;
    [SerializeField] private GameObject armorPiercingBulletPrefab;
    [SerializeField] private float modelDistance = 0.5f;
    [SerializeField] private float tilt = 18f;
    [SerializeField] private float spinSpeed = 60f;
    [SerializeField] private float queueAngle = 35f;
    [SerializeField] private float reloadDimming = 0.6f;

    [SerializeField] private float slideTime = 0.12f;
    [SerializeField] private Vector2 flyVelocity = new Vector2(-260f, 620f);
    [SerializeField] private float flyGravity = 2400f;
    [SerializeField] private float flySpin = 720f;
    [SerializeField] private float flyLifetime = 1.5f;

    private class FlyingBullet
    {
        public GameObject model;
        public Vector2 position;
        public Vector2 velocity;
        public float height;
        public float angle;
        public float age;
    }

    private GameObject[] models;
    private string[] modelTypes;
    private float slideProgress = 1f;
    private float spinAngle;
    private List<FlyingBullet> flying = new List<FlyingBullet>();
    private MaterialPropertyBlock block;
    private Vector3[] corners = new Vector3[4];

    private void Awake()
    {
        models = new GameObject[ammoMarkers.Length];
        modelTypes = new string[ammoMarkers.Length];
        block = new MaterialPropertyBlock();

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
        Camera cam = Camera.main;

        group.SetActive(isAttacking);

        if (!isAttacking || cam == null)
        {
            HideModels();
            return;
        }

        if (turnManager.config != null && turnManager.config.playerTurnDuration > 0f)
            timerFill.fillAmount = Mathf.Clamp01(turnManager.TimeLeft / turnManager.config.playerTurnDuration);

        slideProgress = Mathf.Min(1f, slideProgress + Time.deltaTime / slideTime);
        spinAngle += spinSpeed * Time.deltaTime;

        UpdateQueue(cam);
        UpdateFlying(cam);
    }

    private void UpdateQueue(Camera cam)
    {
        float t = 1f - (1f - slideProgress) * (1f - slideProgress);
        int last = ammoMarkers.Length - 1;
        Color tint = Color.white;

        if (ammo != null && ammo.IsReloading)
            tint = Color.Lerp(Color.white, Color.black, reloadDimming);

        for (int i = 0; i < ammoMarkers.Length; i++)
        {
            bool hasBullet = ammo != null && i < ammo.Queue.Count;

            if (!hasBullet)
            {
                SetModelActive(i, false);
                continue;
            }

            GameObject model = GetModel(i, ammo.Queue[i]);
            model.SetActive(true);

            Vector2 toPosition = GetScreenCenter(ammoMarkers[i]);
            float toHeight = GetScreenHeight(ammoMarkers[i]);
            Vector2 fromPosition = toPosition;
            float fromHeight = toHeight;

            if (i < last)
            {
                fromPosition = GetScreenCenter(ammoMarkers[i + 1]);
                fromHeight = GetScreenHeight(ammoMarkers[i + 1]);
            }
            else if (last > 0)
            {
                fromPosition = toPosition + (toPosition - GetScreenCenter(ammoMarkers[last - 1]));
            }

            float angle = queueAngle;

            if (i == 0)
                angle = spinAngle;

            PlaceModel(cam, model.transform, Vector2.Lerp(fromPosition, toPosition, t), Mathf.Lerp(fromHeight, toHeight, t), Quaternion.Euler(0f, angle, 0f));
            SetTint(model, tint);
        }
    }

    private void UpdateFlying(Camera cam)
    {
        float scale = Screen.height / 1080f;

        for (int i = flying.Count - 1; i >= 0; i--)
        {
            FlyingBullet bullet = flying[i];
            bullet.age += Time.deltaTime;
            bullet.velocity.y -= flyGravity * scale * Time.deltaTime;
            bullet.position += bullet.velocity * Time.deltaTime;
            bullet.angle += flySpin * Time.deltaTime;

            PlaceModel(cam, bullet.model.transform, bullet.position, bullet.height, Quaternion.Euler(0f, 0f, bullet.angle));

            if (bullet.age >= flyLifetime || bullet.position.y < -bullet.height)
            {
                Destroy(bullet.model);
                flying.RemoveAt(i);
            }
        }
    }

    private void OnFired(string type)
    {
        GameObject prefab = GetPrefab(type);

        if (prefab == null || ammoMarkers.Length == 0)
            return;

        float scale = Screen.height / 1080f;

        FlyingBullet bullet = new FlyingBullet();
        bullet.model = CreateModel(prefab);
        bullet.position = GetScreenCenter(ammoMarkers[0]);
        bullet.height = GetScreenHeight(ammoMarkers[0]);
        bullet.velocity = flyVelocity * scale;
        bullet.angle = 0f;
        flying.Add(bullet);

        slideProgress = 0f;
    }

    private void PlaceModel(Camera cam, Transform model, Vector2 screenPosition, float screenHeight, Quaternion spin)
    {
        Vector3 point = new Vector3(screenPosition.x, screenPosition.y, modelDistance);
        float worldHeight = screenHeight / Screen.height * 2f * modelDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        model.position = cam.ScreenToWorldPoint(point);
        model.rotation = cam.transform.rotation * Quaternion.Euler(-tilt, 0f, 0f) * spin;
        model.localScale = Vector3.one * worldHeight;
    }

    private GameObject GetModel(int index, string type)
    {
        if (models[index] != null && modelTypes[index] == type)
            return models[index];

        if (models[index] != null)
            Destroy(models[index]);

        models[index] = CreateModel(GetPrefab(type));
        modelTypes[index] = type;
        return models[index];
    }

    private GameObject CreateModel(GameObject prefab)
    {
        GameObject model = Instantiate(prefab);
        SceneManager.MoveGameObjectToScene(model, gameObject.scene);
        return model;
    }

    private void SetModelActive(int index, bool isActive)
    {
        if (models[index] != null)
            models[index].SetActive(isActive);
    }

    private void HideModels()
    {
        for (int i = 0; i < models.Length; i++)
        {
            SetModelActive(i, false);
        }

        foreach (FlyingBullet bullet in flying)
        {
            Destroy(bullet.model);
        }

        flying.Clear();
    }

    private void SetTint(GameObject model, Color tint)
    {
        foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
        {
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", tint);
            r.SetPropertyBlock(block);
        }
    }

    private GameObject GetPrefab(string type)
    {
        if (type == "blue")
            return armorPiercingBulletPrefab;

        return normalBulletPrefab;
    }

    private Vector2 GetScreenCenter(RectTransform marker)
    {
        return RectTransformUtility.WorldToScreenPoint(null, marker.TransformPoint(marker.rect.center));
    }

    private float GetScreenHeight(RectTransform marker)
    {
        marker.GetWorldCorners(corners);
        return corners[1].y - corners[0].y;
    }
}
