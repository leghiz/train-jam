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

    void Start()
    {
        camera = GetComponent<Camera>();
        ammo = GetComponent<Ammo>();
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
        Vector3 flatDirection = new Vector3(ray.direction.x, 90f, ray.direction.z);
        Quaternion spawnRotation = Quaternion.LookRotation(flatDirection);

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, spawnRotation);
        IgnorePlayer(bulletObj);

        Renderer renderer = bulletObj.GetComponent<Renderer>();
        if (type == "blue")
            renderer.material.color = Color.blue;
        else
            renderer.material.color = Color.red;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.damage = weapon != null ? weapon.damage : config.playerDamage;
        if (bullet != null) bullet.Init(ray.direction, type);
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
