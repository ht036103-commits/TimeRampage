using UnityEngine;

[RequireComponent(typeof(EnemyActor))]
[RequireComponent(typeof(GroundEnemyMotor))]
public class MageAI : MonoBehaviour
{
    private enum MageState
    {
        Idle,
        Chase,
        Retreat,
        Cast,
        Dead
    }

    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform magicSpawnPoint;
    [SerializeField] private GameObject magicProjectilePrefab;
    [SerializeField] private Animator animator;

    [Header("Distance")]
    [SerializeField] private float detectionRange = 14f;
    [SerializeField] private float castRange = 9f;
    [SerializeField] private float retreatDistance = 4f;
    [SerializeField] private float stopDistance = 7f;

    [Header("Casting")]
    [SerializeField] private float castCooldown = 2.5f;
    [SerializeField] private float castingWindup = 0.7f;
    [SerializeField] private float projectileSpeed = 11f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    private EnemyActor enemyActor;
    private GroundEnemyMotor motor;

    private MageState currentState = MageState.Idle;
    private float nextCastTime;

    private bool isCasting;
    private bool projectileReleased;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int DeadHash =
        Animator.StringToHash("Dead");

    private void Awake()
    {
        enemyActor = GetComponent<EnemyActor>();
        motor = GetComponent<GroundEnemyMotor>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void Update()
    {
        if (enemyActor == null || enemyActor.IsDead)
        {
            EnterDeadState();
            return;
        }

        if (target == null)
        {
            ChangeState(MageState.Idle);
            UpdateAnimator(0f);
            return;
        }

        float distanceToTarget =
            Vector3.Distance(transform.position, target.position);

        if (isCasting)
        {
            ChangeState(MageState.Cast);
            FaceTarget();
            UpdateAnimator(0f);
            return;
        }

        SelectState(distanceToTarget);
        ExecuteCurrentState();
    }

    private void SelectState(float distanceToTarget)
    {
        if (distanceToTarget > detectionRange)
        {
            ChangeState(MageState.Idle);
        }
        else if (distanceToTarget < retreatDistance)
        {
            ChangeState(MageState.Retreat);
        }
        else if (distanceToTarget <= castRange &&
                 Time.time >= nextCastTime)
        {
            StartCasting();
        }
        else if (distanceToTarget > stopDistance)
        {
            ChangeState(MageState.Chase);
        }
        else
        {
            ChangeState(MageState.Idle);
        }
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case MageState.Idle:
                StopMoving();
                FaceTarget();
                break;

            case MageState.Chase:
                MoveTowardsTarget();
                break;

            case MageState.Retreat:
                MoveAwayFromTarget();
                break;
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction =
            GetFlatDirection(target.position - transform.position);

        motor.Move(direction);
        RotateTowards(direction);
        UpdateAnimator(1f);
    }

    private void MoveAwayFromTarget()
    {
        Vector3 direction =
            GetFlatDirection(transform.position - target.position);

        motor.Move(direction);
        RotateTowards(-direction);
        UpdateAnimator(1f);
    }

    private void StopMoving()
    {
        motor.Stop();
        UpdateAnimator(0f);
    }

    private void StartCasting()
    {
        ChangeState(MageState.Cast);

        isCasting = true;
        projectileReleased = false;
        nextCastTime = Time.time + castCooldown;

        motor.Stop();
        FaceTarget();

        if (animator != null)
        {
            animator.SetTrigger(AttackHash);
        }

        Invoke(nameof(ReleaseProjectile), castingWindup);
        Invoke(nameof(FinishCasting), castingWindup + 0.5f);
    }

    private void ReleaseProjectile()
    {
        if (!isCasting ||
            projectileReleased ||
            enemyActor.IsDead ||
            target == null ||
            magicSpawnPoint == null ||
            magicProjectilePrefab == null)
        {
            return;
        }

        projectileReleased = true;

        Vector3 targetPoint = target.position + Vector3.up;
        Vector3 direction =
            (targetPoint - magicSpawnPoint.position).normalized;

        GameObject projectile = Instantiate(
            magicProjectilePrefab,
            magicSpawnPoint.position,
            Quaternion.LookRotation(direction)
        );

        Rigidbody projectileBody =
            projectile.GetComponent<Rigidbody>();

        if (projectileBody != null)
        {
            projectileBody.velocity = direction * projectileSpeed;
        }

        Destroy(projectile, projectileLifetime);
    }

    private void FinishCasting()
    {
        isCasting = false;
        projectileReleased = false;

        if (currentState != MageState.Dead)
        {
            ChangeState(MageState.Idle);
        }
    }

    private void FaceTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 direction =
            GetFlatDirection(target.position - transform.position);

        RotateTowards(direction);
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private Vector3 GetFlatDirection(Vector3 direction)
    {
        direction.y = 0f;
        return direction.normalized;
    }

    private void UpdateAnimator(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        }
    }

    private void ChangeState(MageState newState)
    {
        currentState = newState;
    }

    private void EnterDeadState()
    {
        if (currentState == MageState.Dead)
        {
            return;
        }

        currentState = MageState.Dead;
        isCasting = false;

        CancelInvoke();
        motor.Stop();

        if (animator != null)
        {
            animator.SetBool(DeadHash, true);
        }

        enabled = false;
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, castRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}