using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class Shooting : MonoBehaviour
{
    [Header("Пуля")]
    public GameObject bulletPrefab;

    [Header("Стрельба")]
    public float fireRate = 1f;
    public float spawnOffset = 0.1f;

    private float nextFireTime;
    private Camera camera;
    private string[] bulletType = { "blue", "blue", "red", "blue", "red" };
    private int currentIndex = 0;

    void Start()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (TurnManager.Instance == null || TurnManager.Instance.CurrentTurn != TurnManager.Turn.Player)
            return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot(GetNextBulletType());
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot(string type)
    {
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 spawnPos = ray.origin + ray.direction * spawnOffset;
        Quaternion spawnRotation = Quaternion.LookRotation(ray.direction);

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, spawnRotation);
        IgnorePlayer(bulletObj);

        Renderer renderer = bulletObj.GetComponent<Renderer>();
        if (type == "blue")
            renderer.material.color = Color.blue;
        else
            renderer.material.color = Color.red;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
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
        string type = bulletType[currentIndex];
        currentIndex = (currentIndex + 1) % bulletType.Length;

        return type;
    }
}
