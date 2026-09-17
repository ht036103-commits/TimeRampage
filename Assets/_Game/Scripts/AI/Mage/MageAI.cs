using UnityEngine;

[RequireComponent(typeof(EnemyActor))]
[RequireComponent(typeof(GroundEnemyMotor))]
public sealed class MageAI : MonoBehaviour
{
    private enum MageState
    {
        Idle,
        Chase,
        Retreat,
        Cast
    }

    [Header("References")]
    [SerializeField] private EnemyActor actor;
    [SerializeField] private GroundEnemyMotor motor;
    [SerializeField] private Transform target;
    [SerializeField] private Transform magicSpawnPoint;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    [Header("Distance")]
    [SerializeField, Min(0f)] private float detectionRange = 14f;
    [SerializeField, Min(0f)] private float verticalTolerance = 8f;
    [SerializeField, Min(0f)] private float castRange = 9f;
    [SerializeField, Min(0f)] private float retreatDistance = 4f;
    [SerializeField, Min(0f)] private float stopDistance = 7f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float chaseSpeed = 3.5f;
    [SerializeField, Min(0f)] private float retreatSpeed = 4f;
    [SerializeField, Min(0f)] private float groundCheckForward = 0.7f;
    [SerializeField, Min(0f)] private float groundCheckDown = 1.2f;

    [Header("Casting")]
    [SerializeField, Min(1)] private int projectileDamage = 12;
    [SerializeField, Min(0.1f)] private float castCooldown = 2.5f;
    [SerializeField, Min(0f)] private float castingWindup = 0.7f;
    [SerializeField, Min(0f)] private float castingRecovery = 0.5f;
    [SerializeField, Min(0f)] private float projectileSpeed = 11f;
    [SerializeField, Min(0.1f)] private float projectileLifetime = 5f;

    private MageState state = MageState.Idle;
    private float nextCastTime;
    private float castTimer;
    private bool projectileReleased;

    private void Awake()
    {
        actor ??= GetComponent<EnemyActor>();
        motor ??= GetComponent<GroundEnemyMotor>();
    }

    private void Start()
    {
        FindTargetIfNecessary();
    }

    private void Update()
    {
        if (actor == null || motor == null)
        {
            enabled = false;
            return;
        }

        if (actor.IsDead)
        {
            motor.StopImmediately();
            enabled = false;
            return;
        }

        FindTargetIfNecessary();

        if (target == null)
        {
            StopMoving();
            return;
        }

        if (state == MageState.Cast)
        {
            UpdateCasting();
            return;
        }

        float horizontalDistance =
            Mathf.Abs(target.position.x - transform.position.x);
        float verticalDistance =
            Mathf.Abs(target.position.y - transform.position.y);

        FaceTarget();

        if (horizontalDistance > detectionRange ||
            verticalDistance > verticalTolerance)
        {
            ChangeState(MageState.Idle);
            StopMoving();
            return;
        }

        if (horizontalDistance < retreatDistance)
        {
            ChangeState(MageState.Retreat);
            Move(-DirectionToTarget(), retreatSpeed);
            return;
        }

        if (horizontalDistance <= castRange &&
            Time.time >= nextCastTime)
        {
            BeginCasting();
            return;
        }

        if (horizontalDistance > stopDistance)
        {
            ChangeState(MageState.Chase);
            Move(DirectionToTarget(), chaseSpeed);
            return;
        }

        ChangeState(MageState.Idle);
        StopMoving();
    }

    private void Move(float direction, float speed)
    {
        if (!HasGroundAhead(direction))
        {
            StopMoving();
            return;
        }

        motor.Move(direction, speed);
        motor.Face(direction);
        actor.SetMovementSpeed(1f);
    }

    private void StopMoving()
    {
        motor.Stop();
        actor.SetMovementSpeed(0f);
    }

    private void BeginCasting()
    {
        ChangeState(MageState.Cast);
        castTimer = 0f;
        projectileReleased = false;
        nextCastTime = Time.time + castCooldown;

        motor.StopImmediately();
        actor.SetMovementSpeed(0f);
        actor.PlayAttack();
    }

    private void UpdateCasting()
    {
        motor.Stop();
        FaceTarget();
        castTimer += Time.deltaTime;

        if (!projectileReleased &&
            castTimer >= castingWindup)
        {
            ReleaseProjectile();
            projectileReleased = true;
        }

        if (castTimer >= castingWindup + castingRecovery)
        {
            ChangeState(MageState.Idle);
        }
    }

    private void ReleaseProjectile()
    {
        if (target == null ||
            magicSpawnPoint == null ||
            projectilePool == null)
        {
            return;
        }

        Projectile projectile = projectilePool.Get();

        if (projectile == null)
        {
            return;
        }

        Vector3 targetPoint = target.position + Vector3.up;
        Vector3 direction =
            (targetPoint - magicSpawnPoint.position).normalized;

        projectile.Launch(
            transform,
            magicSpawnPoint.position,
            direction,
            projectileDamage,
            projectileSpeed,
            projectileLifetime,
            projectilePool.Release
        );
    }

    private float DirectionToTarget()
    {
        return Mathf.Sign(target.position.x - transform.position.x);
    }

    private void FaceTarget()
    {
        float direction = DirectionToTarget();

        if (Mathf.Abs(direction) > 0.001f)
        {
            motor.Face(direction);
        }
    }

    private bool HasGroundAhead(float direction)
    {
        if (groundCheck == null || Mathf.Abs(direction) < 0.001f)
        {
            return true;
        }

        Vector3 origin =
            groundCheck.position +
            Vector3.right * Mathf.Sign(direction) * groundCheckForward;

        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDown,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void FindTargetIfNecessary()
    {
        if (target != null)
        {
            return;
        }

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
    }

    private void ChangeState(MageState newState)
    {
        state = newState;
    }

    private void OnDisable()
    {
        if (motor != null)
        {
            motor.StopImmediately();
        }
    }

    private void OnValidate()
    {
        detectionRange = Mathf.Max(0f, detectionRange);
        verticalTolerance = Mathf.Max(0f, verticalTolerance);
        castRange = Mathf.Max(0f, castRange);
        retreatDistance = Mathf.Max(0f, retreatDistance);
        stopDistance = Mathf.Clamp(stopDistance, retreatDistance, castRange);
        chaseSpeed = Mathf.Max(0f, chaseSpeed);
        retreatSpeed = Mathf.Max(0f, retreatSpeed);
        projectileDamage = Mathf.Max(1, projectileDamage);
        castCooldown = Mathf.Max(0.1f, castCooldown);
        projectileLifetime = Mathf.Max(0.1f, projectileLifetime);
    }
}
