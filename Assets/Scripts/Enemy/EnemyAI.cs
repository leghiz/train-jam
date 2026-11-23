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
    private Animator animator;
    private EnemyEncounter encounter;
    private NoteView noteView;

    private float memoryTimer;
    private float fireTimer;

    private Vector3 lastKnownPlayerPos;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        animator = GetComponentInChildren<Animator>();
        encounter = GetComponent<EnemyEncounter>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        noteView = FindAnyObjectByType<NoteView>();
    }

    private void Update()
    {
        if (player == null)
            return;
        
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleStarted)
        {
            movement.StopMovement();

            if (animator != null)
            {
                animator.SetBool("IsChasing", false);
                animator.SetBool("IsShooting", false);
            }

            return;
        }

        if (noteView != null && noteView.IsOpen)
        {
            LosePlayer();
            return;
        }

        fireTimer -= Time.deltaTime;

        bool canSeePlayer = CheckForPlayer();

        if (canSeePlayer)
        {
            memoryTimer = memoryDuration;
            lastKnownPlayerPos = player.position;
        }
        else
        {
            memoryTimer -= Time.deltaTime;
        }

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

        if (animator != null)
        {
            animator.SetBool("IsChasing", currentState == State.Chase);
            animator.SetBool("IsShooting", currentState == State.Attack);
        }

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

    private void LosePlayer()
    {
        memoryTimer = 0f;
        currentState = State.Patrol;

        if (animator != null)
        {
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsShooting", false);
        }

        movement.ResumePatrol();
    }

    private bool CheckForPlayer()
    {
        Vector3 eyePosition = transform.position + Vector3.up;
        Vector3 playerPosition = player.position + Vector3.up;
        Vector3 directionToPlayer = playerPosition - eyePosition;

        float distance = directionToPlayer.magnitude;

        if (distance > viewRadius)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > viewAngle / 2f)
            return false;

        if (Physics.Raycast(eyePosition, directionToPlayer.normalized, distance, environmentLayer))
            return false;

        return true;
    }

    private void HandleChase()
    {
        Vector3 targetPosition = memoryTimer > 0f
            ? lastKnownPlayerPos
            : player.position;

        movement.MoveTo(targetPosition);
    }

    private void HandleAttack()
    {
        movement.StopMovement();

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

        if (Physics.Raycast(origin, direction, out RaycastHit hit, attackRange, ~0, QueryTriggerInteraction.Ignore))
        {
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
        movement.StopMovement();

        if (animator != null)
        {
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsShooting", false);
        }

        if (BattleManager.Instance != null && encounter != null)
        {
            BattleManager.Instance.StartBattle(encounter, true);
        }
        else
        {
            Debug.LogWarning("EnemyAI: BattleManager.Instance или EnemyEncounter не найден!", gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward * viewRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward * viewRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
