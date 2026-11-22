using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCombatTargeting : MonoBehaviour
{
    [Header("Target Search")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float detectionAngle = 60f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Close Range Search")]
    [SerializeField] private float closeDetectionRadius = 3f;

    [Header("Targeting")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Vector3 aimOriginOffset = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 1f, 0);

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private Animator animator;

    private Rigidbody rb;

    private bool isAiming;
    private EnemyEncounter currentTarget;

    public bool IsAiming => isAiming;
    public EnemyEncounter CurrentTarget => currentTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerRotation == null)
            playerRotation = GetComponent<PlayerRotation>();
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        if (mouse.rightButton.wasPressedThisFrame)
            StartAiming();

        if (mouse.rightButton.wasReleasedThisFrame)
            StopAiming();

        if (!isAiming)
            return;

        FindTarget();

        if (mouse.leftButton.wasPressedThisFrame)
            Shoot();
    }

    private void FixedUpdate()
    {
        if (!isAiming || currentTarget == null)
            return;

        RotateTowardsTarget();
    }

    private void StartAiming()
    {
        isAiming = true;

        currentTarget = null;
        
        if (animator != null)
            animator.SetBool("IsAiming", true);
        
        if (playerMovement != null)
            playerMovement.enabled = false;
        
        if (playerRotation != null)
            playerRotation.enabled = false;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void StopAiming()
    {
        isAiming = false;
        currentTarget = null;
        
        if (animator != null)
            animator.SetBool("IsAiming", false); 
        
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerRotation != null)
            playerRotation.enabled = true;
    }

    private void FindTarget()
    {
        if (currentTarget != null)
        {
            if (IsTargetValid(currentTarget))
                return;

            currentTarget = null;
        }
        
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        EnemyEncounter bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            EnemyEncounter enemy = col.GetComponentInParent<EnemyEncounter>();

            if (enemy == null)
                continue;

            Vector3 direction = enemy.transform.position - transform.position;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance <= 0.001f)
                continue;
            
            bool insideMainSector =
                distance <= detectionRadius &&
                Vector3.Angle(transform.forward, direction) <= detectionAngle;
            
            bool insideCloseRadius = distance <= closeDetectionRadius;

            if (!insideMainSector && !insideCloseRadius)
                continue;
            
            if (!HasLineOfSight(enemy))
                continue;
            
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = enemy;
            }
        }

        currentTarget = bestTarget;
    }

    private bool IsTargetValid(EnemyEncounter enemy)
    {
        if (enemy == null)
            return false;

        Vector3 direction = enemy.transform.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > detectionRadius)
            return false;

        bool insideMainSector =
            Vector3.Angle(transform.forward, direction) <= detectionAngle;

        bool insideCloseRadius = distance <= closeDetectionRadius;

        if (!insideMainSector && !insideCloseRadius)
            return false;

        return HasLineOfSight(enemy);
    }

    private bool HasLineOfSight(EnemyEncounter enemy)
    {
        Vector3 origin = transform.position + aimOriginOffset;
        Vector3 targetPosition = enemy.transform.position + targetOffset;

        Vector3 direction = targetPosition - origin;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
            return true;
        
        return !Physics.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = currentTarget.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            )
        );
    }

    private void Shoot()
    {
        if (currentTarget == null)
        {
            return;
        }
        
        BattleManager.Instance.StartBattle(currentTarget);
    }
}