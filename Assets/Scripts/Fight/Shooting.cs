using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class Shooting : MonoBehaviour
{
    [Header("Пуля")]
    public GameObject bulletPrefab;

    [Header("Стрельба")]
    public float fireRate = 1f;
    public float bulletSpeed = 15f;
    public float bulletLifetime = 3f;
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
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot(GetNextBulletType());
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot(string type)
    {
        Vector3 spawnPos = camera.transform.position + camera.transform.forward * spawnOffset;
        Quaternion spawnRotation = camera.transform.rotation;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null) 
            bulletComp.type = type;

        Renderer renderer = bullet.GetComponent<Renderer>();
        if (type == "blue")
            renderer.material.color = Color.blue;
        else
            renderer.material.color = Color.red;

        Vector3 velocity = spawnRotation * Vector3.forward * bulletSpeed;
        rb.linearVelocity = velocity;
    }

    private string GetNextBulletType()
    {
        string type = bulletType[currentIndex];
        currentIndex = (currentIndex + 1) % bulletType.Length;

        return type;
    }
}
