using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("����")]
    public GameObject bulletPrefab;

    [Header("����� �������� �����")]
    public float spawnOffset = 0.5f;

    [Header("������")]
    public BattleConfig config;
    
    [Header("Анимация")]
    [SerializeField] private Animator animator;
    
    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        if (animator != null)
        {
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsShooting", false);
        }
    }
    
    private void OnEnable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnChanged += HandleTurnChanged;
    }

    private void OnDisable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnChanged -= HandleTurnChanged;
    }
    
    private void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged -= HandleTurnChanged;
            TurnManager.Instance.OnTurnChanged += HandleTurnChanged;
            HandleTurnChanged(TurnManager.Instance.CurrentTurn);
        }
    }
    
    private void HandleTurnChanged(TurnManager.Turn currentTurn)
    {
        if (animator == null) return;

        bool isEnemyTurn = (currentTurn == TurnManager.Turn.Enemy);
        animator.SetBool("IsShooting", isEnemyTurn);
    }

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

        PlayShotEffects(spawnPos, dir);
    }

    private void PlayShotEffects(Vector3 position, Vector3 direction)
    {
        if (config.shotSmokePrefab != null)
            Instantiate(config.shotSmokePrefab, position, Quaternion.LookRotation(direction));

        if (config.shotSound != null)
            AudioSource.PlayClipAtPoint(config.shotSound, position, config.shotVolume);

        Camera camera = Camera.main;

        if (camera != null)
        {
            CameraShake shake = camera.GetComponent<CameraShake>();

            if (shake != null)
                shake.Shake(config.enemyShotShake);
        }

        StartCoroutine(PlayShell(position));
    }

    private IEnumerator PlayShell(Vector3 position)
    {
        yield return new WaitForSeconds(config.shellDelay);

        if (config.shellSound != null)
            AudioSource.PlayClipAtPoint(config.shellSound, position, config.shellVolume);
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
