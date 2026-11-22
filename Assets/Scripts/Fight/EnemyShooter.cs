using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Пуля")]
    public GameObject bulletPrefab;

    [Header("Точка выстрела врага")]
    public float spawnOffset = 0.5f;

    [Header("Конфиг")]
    public BattleConfig config;

    public void Shoot()
    {
        GameObject player = FindPlayer();
        if (player == null)
            return;

        Vector3 target = player.transform.position;
        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider != null)
            target = playerCollider.bounds.center;

        Vector3 dir = (target - transform.position).normalized;
        Vector3 spawnPos = transform.position + dir * spawnOffset;

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.damage = config.enemyDamage;
        if (bullet != null) 
            bullet.Init(dir, "red");
    }

    private GameObject FindPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.scene == gameObject.scene)
                return player;
        }

        return null;
    }
}
