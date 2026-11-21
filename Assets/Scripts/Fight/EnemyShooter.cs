using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Пуля")]
    public GameObject bulletPrefab;

    [Header("Точка выстрела врага")]
    public float spawnOffset = 0.5f;

    public void Shoot()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Vector3 dir = (player.transform.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + dir * spawnOffset;

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null) 
            bullet.Init(dir, "red");
    }
}
