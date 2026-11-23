using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class Shooting : MonoBehaviour
{
    [Header("����")]
    public GameObject bulletPrefab;

    [Header("��������")]
    public BattleConfig config;
    public float spawnOffset = 0.1f;

    private float nextFireTime;
    private Camera camera;
    private Ammo ammo;
    private string[] bulletType = { "blue", "blue", "red", "blue", "red" };
    private int currentIndex = 0;
    private WeaponData weapon;
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private float muzzleForward = 0.35f;
    [SerializeField] private float aimDistance = 50f;

    private CameraShake shake;
    private Transform muzzle;
    private Crosshair crosshairView;

    public float ReloadProgress
    {
        get
        {
            if (ammo != null && ammo.IsReloading && ammo.ReloadTime > 0f)
                return Mathf.Clamp01(1f - ammo.ReloadTimeLeft / ammo.ReloadTime);

            if (config.fireRate <= 0f)
                return 1f;

            return Mathf.Clamp01(1f - (nextFireTime - Time.time) / config.fireRate);
        }
    }

    void Start()
    {
        camera = GetComponent<Camera>();
        ammo = GetComponent<Ammo>();
        shake = GetComponent<CameraShake>();

        if (crosshair != null)
            crosshairView = crosshair.GetComponent<Crosshair>();
        weapon = BattleContext.Weapon;
    }

    void Update()
    {
        if (TurnManager.Instance == null || TurnManager.Instance.CurrentTurn != TurnManager.Turn.Player)
            return;

        if (!TurnManager.Instance.AttackChosen)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (ammo != null && !ammo.CanShoot)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            Shoot(GetNextBulletType());
            nextFireTime = Time.time + config.fireRate;
        }
    }

    void Shoot(string type)
    {
        Ray ray = camera.ScreenPointToRay(crosshair.position);
        Vector3 spawnPos = ray.origin + ray.direction * spawnOffset;
        Vector3 direction = ray.direction;

        Transform weaponHolder = FindMuzzle();

        if (weaponHolder != null)
        {
            direction = (FindAimPoint(ray) - weaponHolder.position).normalized;
            spawnPos = weaponHolder.position + direction * muzzleForward;
        }

        Quaternion spawnRotation = Quaternion.LookRotation(direction);

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, spawnRotation);
        IgnorePlayer(bulletObj);

        Renderer renderer = bulletObj.GetComponent<Renderer>();
        if (type == "blue")
            renderer.material.color = Color.blue;
        else
            renderer.material.color = Color.red;

        TrailRenderer trail = bulletObj.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            Color tracerColor = new Color(1f, 0.55f, 0.2f, 1f);

            if (type == "blue")
                tracerColor = new Color(0.35f, 0.7f, 1f, 1f);

            trail.startColor = tracerColor;
            trail.endColor = new Color(tracerColor.r, tracerColor.g, tracerColor.b, 0f);
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.damage = weapon != null ? weapon.damage : config.playerDamage;
        if (bullet != null) bullet.Init(direction, type);

        PlayShotEffects(spawnPos, direction);
    }

    private Transform FindMuzzle()
    {
        if (muzzle != null)
            return muzzle;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.scene != gameObject.scene)
                continue;

            WeaponVisuals visuals = player.GetComponentInChildren<WeaponVisuals>();

            if (visuals != null)
                muzzle = visuals.WeaponHolder;
        }

        return muzzle;
    }

    private Vector3 FindAimPoint(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, aimDistance, ~0, QueryTriggerInteraction.Ignore);

        Vector3 point = ray.origin + ray.direction * aimDistance;
        float bestDistance = aimDistance;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.GetComponentInParent<PlayerStats>() != null)
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                point = hit.point;
            }
        }

        return point;
    }

    private void PlayShotEffects(Vector3 position, Vector3 direction)
    {
        if (config.shotSmokePrefab != null)
            Instantiate(config.shotSmokePrefab, position, Quaternion.LookRotation(direction));

        if (config.shotSound != null)
            AudioSource.PlayClipAtPoint(config.shotSound, transform.position, config.shotVolume);

        if (shake != null)
        {
            shake.Shake(config.shotShake);
            shake.Kick(config.shotKick);
        }

        if (crosshairView != null)
            crosshairView.Kick();

        StartCoroutine(PlayAfterShot());
    }

    private IEnumerator PlayAfterShot()
    {
        yield return new WaitForSeconds(config.shellDelay);

        if (config.shellSound != null)
            AudioSource.PlayClipAtPoint(config.shellSound, transform.position, config.shellVolume);

        if (config.reloadSound == null)
            yield break;

        float cooldown = config.fireRate;

        if (ammo != null && ammo.ReloadTime > cooldown)
            cooldown = ammo.ReloadTime;

        float wait = cooldown - config.shellDelay - config.reloadSound.length;

        if (wait > 0f)
            yield return new WaitForSeconds(wait);

        AudioSource.PlayClipAtPoint(config.reloadSound, transform.position, config.reloadVolume);
    }

    private void IgnorePlayer(GameObject bulletObj)
    {
        Collider bulletCollider = bulletObj.GetComponent<Collider>();
        if (bulletCollider == null)
            return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.scene != gameObject.scene)
                continue;

            foreach (Collider playerCollider in player.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(bulletCollider, playerCollider);
            }
        }
    }

    private string GetNextBulletType()
    {
        if (ammo != null)
            return ammo.TakeNext();

        string type = bulletType[currentIndex];
        currentIndex = (currentIndex + 1) % bulletType.Length;

        return type;
    }
}
