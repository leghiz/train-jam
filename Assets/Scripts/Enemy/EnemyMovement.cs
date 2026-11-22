
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("Patrol")]
    public float patrolRadius = 10f;
    public float patrolSpeed = 2.5f;
    public float waitTime = 5f;

    [Header("Movement")]
    public float chaseSpeed = 4.5f;
    public float rotationSpeed = 8f;
    public float targetReachedDistance = 1f;

    [Header("Patrol Target Search")]
    public int maxAttempts = 20;
    public float navMeshSearchRadius = 2f;

    private NavMeshAgent agent;

    private Vector3 patrolCenter;
    private Vector3 currentTarget;

    private float waitTimer;

    private bool isWaiting;
    private bool isPatrolling;
    private bool hasTarget;
    
    private Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("EnemyMovement: NavMeshAgent не найден!", gameObject);
            enabled = false;
            return;
        }

        // Ищем ближайшую точку NavMesh около врага
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(
                    transform.position,
                    out NavMeshHit spawnHit,
                    5f,
                    agent.areaMask))
            {
                agent.Warp(spawnHit.position);
            }
            else
            {
                Debug.LogError("EnemyMovement: рядом со врагом не найден NavMesh!", gameObject);
                enabled = false;
                return;
            }
        }

        patrolCenter = transform.position;

        agent.speed = patrolSpeed;
        agent.angularSpeed = rotationSpeed * 100f;
        agent.stoppingDistance = targetReachedDistance;
        agent.updateRotation = true;

        isPatrolling = true;

        SetNewPatrolTarget();
    }

    private void Update()
    {
        if (animator != null && agent != null)
        {
            bool isMoving = !agent.isStopped && agent.velocity.magnitude > 0.1f;
            animator.SetBool("IsMoving", isMoving);
        }
        
        if (!isPatrolling)
            return;

        // Если враг отдыхает на точке
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                SetNewPatrolTarget();
            }

            return;
        }
        
        // Если враг достиг точки патрулирования
        if (hasTarget && !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            // Убеждаемся, что агент действительно остановился
            if (agent.velocity.sqrMagnitude < 0.01f)
            {
                StartWaiting();
            }
        }
    }

    private void StartWaiting()
    {
        if (isWaiting)
            return;
        
        agent.isStopped = true;

        hasTarget = false;
        isWaiting = true;
        waitTimer = waitTime;
    }

    public void SetNewPatrolTarget()
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            // Случайная точка в радиусе патрулирования
            Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;

            Vector3 candidate = patrolCenter +
                new Vector3(randomPoint.x, 0f, randomPoint.y);

            // Проверяем, есть ли рядом доступная точка NavMesh
            if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                navMeshSearchRadius,
                agent.areaMask))
            {
                continue;
            }

            // Проверяем, можно ли построить полный маршрут
            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            // Если точка доступна, идём к ней
            currentTarget = hit.position;

            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.updateRotation = true;
            agent.SetPath(path);

            hasTarget = true;
            isWaiting = false;

            return;
        }

        // Если подходящую точку найти не удалось
        Debug.Log("EnemyMovement: не удалось найти точку патруля.");

        isWaiting = true;
        hasTarget = false;
        waitTimer = waitTime;
    }

    public void MoveTo(Vector3 destination)
    {
        if (!IsAgentReady())
            return;
        
        isPatrolling = false;
        isWaiting = false;
        waitTimer = 0;
        hasTarget = true;

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.speed = chaseSpeed;

        // Не перестраиваем маршрут без необходимости
        if (agent.hasPath &&
            Vector3.Distance(agent.destination, destination) < 0.5f)
        {
            return;
        }

        agent.SetDestination(destination);
    }

    public void StopMovement()
    {
        isPatrolling = false;
        isWaiting = false;
        hasTarget = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
        }
    }

    public void ResumePatrol()
    {
        if (!IsAgentReady())
            return;

        if (isPatrolling)
            return;

        isPatrolling = true;
        isWaiting = false;
        hasTarget = false;

        agent.speed = patrolSpeed;
        agent.updateRotation = true;

        SetNewPatrolTarget();
    }

    public bool IsMoving()
    {
        return agent != null &&
               !agent.isStopped &&
               agent.velocity.sqrMagnitude > 0.1f;
    }
    
    private bool IsAgentReady()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? patrolCenter : transform.position,
            patrolRadius
        );

        if (hasTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(currentTarget, 0.25f);
        }
    }
}
