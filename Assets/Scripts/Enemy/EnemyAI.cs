
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("State")]
    public State currentState = State.Patrol;

    [Header("Player Detection")]
    public Transform player;
    public float viewRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 120f;
    public float memoryDuration = 4f;
    public LayerMask environmentLayer;

    [Header("Combat")]
    public float attackRange = 3f;
    public float fireCooldown = 3f;
    public Transform firePoint;

    [Header("Rotation")]
    public float rotationSpeed = 8f;

    private EnemyMovement movement;

    private float memoryTimer;
    private float fireTimer;

    private Vector3 lastKnownPlayerPos;

    private bool battleStarted;

    private void Start()
    {
        movement = GetComponent<EnemyMovement>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null || battleStarted)
            return;

        fireTimer -= Time.deltaTime;

        bool canSeePlayer = CheckForPlayer();

        // Если игрок виден, обновляем его последнюю известную позицию
        if (canSeePlayer)
        {
            memoryTimer = memoryDuration;
            lastKnownPlayerPos = player.position;
        }
        else
        {
            memoryTimer -= Time.deltaTime;
        }

        // Выбираем состояние врага
        if (canSeePlayer)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            State prevState = currentState;
            currentState = distance <= attackRange
                ? State.Attack
                : State.Chase;
            
            if (currentState == State.Attack && prevState != State.Attack)
                fireTimer = fireCooldown;
        }
        else if (memoryTimer > 0f)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }

        // Выполняем поведение текущего состояния
        switch (currentState)
        {
            case State.Patrol:
                movement.ResumePatrol();
                break;

            case State.Chase:
                HandleChase();
                break;

            case State.Attack:
                HandleAttack();
                break;
        }
    }

    private bool CheckForPlayer()
    {
        Vector3 eyePosition = transform.position + Vector3.up;

        Vector3 playerPosition = player.position + Vector3.up;
        Vector3 directionToPlayer = playerPosition - eyePosition;

        float distance = directionToPlayer.magnitude;

        // Проверка дистанции
        if (distance > viewRadius)
            return false;

        // Проверка угла обзора
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > viewAngle / 2f)
            return false;

        // Проверка препятствий
        if (Physics.Raycast(
            eyePosition,
            directionToPlayer.normalized,
            distance,
            environmentLayer))
        {
            return false;
        }

        return true;
    }

    private void HandleChase()
    {
        // Пока игрок виден, идём непосредственно к нему.
        // Если потеряли видимость, идём к последней известной позиции.
        Vector3 targetPosition = memoryTimer > 0f
            ? lastKnownPlayerPos
            : player.position;

        movement.MoveTo(targetPosition);
    }

    private void HandleAttack()
    {
        // Останавливаем движение
        movement.StopMovement();

        // Поворачиваемся к игроку
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Стреляем с интервалом
        
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireCooldown;
        }
    }

    private void Shoot()
    {
        Vector3 origin = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up;

        Vector3 targetPosition = player.position + Vector3.up;
        Vector3 direction = (targetPosition - origin).normalized;

        // Проверяем, куда реально попал выстрел
        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            attackRange,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            // Проверяем, что первым попаданием был игрок
            if (hit.transform.root == player.root)
            {
                Debug.Log("Враг попал в игрока! Запуск боя.");

                StartBattle();
            }
            else
            {
                Debug.Log("Враг промахнулся или попал в препятствие.");
            }
        }
    }

    private void StartBattle()
    {
        battleStarted = true;

        movement.StopMovement();

        // TODO:
        // Здесь позже добавим вызов BattleManager
        // после того, как ты скинешь его код.
    }

    private void OnDrawGizmosSelected()
    {
        // Радиус зрения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Дистанция атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Конус обзора
        Vector3 leftBoundary =
            Quaternion.Euler(0, -viewAngle / 2f, 0)
            * transform.forward * viewRadius;

        Vector3 rightBoundary =
            Quaternion.Euler(0, viewAngle / 2f, 0)
            * transform.forward * viewRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position,
            transform.position + leftBoundary);

        Gizmos.DrawLine(transform.position,
            transform.position + rightBoundary);
    }
}
