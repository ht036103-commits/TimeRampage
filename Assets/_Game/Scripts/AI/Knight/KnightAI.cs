using UnityEngine;

public sealed class KnightAI : MonoBehaviour
{
    private enum KnightState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("References")]
    [SerializeField] private EnemyActor actor;
    [SerializeField] private GroundEnemyMotor motor;
    [SerializeField] private EnemySensors sensors;
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float verticalTolerance = 2f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float loseTargetRange = 12f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 50;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private float attackVerticalTolerance = 1f;
    [SerializeField] private float attackHeight = 0.3f;
    [SerializeField] private float attackRadius = 0.75f;
    [SerializeField] private float attackWindup = 0.3f;
    [SerializeField] private float attackRecovery = 0.55f;
    [SerializeField] private float attackCooldown = 1.1f;
    [SerializeField] private LayerMask playerMask;

    private KnightState state =
        KnightState.Patrol;

    private float patrolDirection = 1f;
    private float cooldownTimer;
    private float attackTimer;
    private bool attackDamageApplied;
    private AnimationEventRelay animationEventRelay;

    private void Awake()
    {
        actor ??= GetComponent<EnemyActor>();
        motor ??= GetComponent<GroundEnemyMotor>();
        sensors ??= GetComponent<EnemySensors>();

        Animator knightAnimator =
            GetComponentInChildren<Animator>(true);

        if (knightAnimator != null)
        {
            animationEventRelay =
                knightAnimator.GetComponent<AnimationEventRelay>();
        }
    }

    private void OnEnable()
    {
        if (animationEventRelay != null)
        {
            animationEventRelay.AttackHit += HandleAttackHit;
        }

        if (actor != null)
        {
            actor.Damaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (animationEventRelay != null)
        {
            animationEventRelay.AttackHit -= HandleAttackHit;
        }

        if (actor != null)
        {
            actor.Damaged -= HandleDamaged;
        }
    }

    private void Start()
    {
        FindPlayerIfNecessary();
    }

    private void Update()
    {
        if (actor == null || motor == null || sensors == null)
        {
            enabled = false;
            return;
        }

        if (actor.IsDead)
        {
            motor.StopImmediately();
            return;
        }

        FindPlayerIfNecessary();

        cooldownTimer = Mathf.Max(
            0f,
            cooldownTimer - Time.deltaTime
        );

        switch (state)
        {
            case KnightState.Patrol:
                UpdatePatrol();
                break;

            case KnightState.Chase:
                UpdateChase();
                break;

            case KnightState.Attack:
                UpdateAttack();
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (CanDetectPlayer(detectionRange))
        {
            state = KnightState.Chase;
            return;
        }

        bool groundAhead =
            sensors.HasGroundAhead(
                patrolDirection
            );

        bool wallAhead =
            sensors.HasWallAhead(
                patrolDirection
            );

        if (!groundAhead || wallAhead)
        {
            patrolDirection *= -1f;
            motor.Face(patrolDirection);
            motor.Stop();
            actor.SetMovementSpeed(0f);
            return;
        }

        motor.Move(
            patrolDirection,
            patrolSpeed
        );

        actor.SetMovementSpeed(
            patrolSpeed / chaseSpeed
        );
    }

    
    private void UpdateChase()
    {
        if (!CanDetectPlayer(loseTargetRange))
        {
            state = KnightState.Patrol;
            motor.Stop();
            actor.SetMovementSpeed(0f);
            return;
        }

        Vector3 playerDifference =
            player.position -
            transform.position;

        float horizontalDistance =
            Mathf.Abs(playerDifference.x);

        float verticalDistance =
            Mathf.Abs(playerDifference.y);

        float direction =
            Mathf.Sign(playerDifference.x);

        if (
            horizontalDistance <= attackRange &&
            verticalDistance <= attackVerticalTolerance
        )
        {
           
            motor.Face(direction);
            motor.StopImmediately();

            actor.SetMovementSpeed(0f);

            if (cooldownTimer <= 0f)
            {
                BeginAttack();
            }

            return;
        }

        bool groundAhead =
            sensors.HasGroundAhead(direction);

        bool wallAhead =
            sensors.HasWallAhead(direction);

        if (!groundAhead || wallAhead)
        {
            motor.Face(direction);
            motor.Stop();
            actor.SetMovementSpeed(0f);
            return;
        }
  
        motor.Face(direction);
        motor.Move(direction, chaseSpeed);
        actor.SetMovementSpeed(1f);

    }

    private void HandleDamaged()
    {
        // Chi huy don tan cong neu Knight dang Attack.
        if (state != KnightState.Attack)
        {
            return;
        }

        // Huy don tan cong hien tai.
        state = KnightState.Chase;

        // Xoa bo dem thoi gian tan cong.
        attackTimer = 0f;

        // Xoa co gay sat thuong.
        attackDamageApplied = false;

        // Cho Knight nghi truoc khi tan cong lai.
        cooldownTimer = attackCooldown;

        // Dung chuyen dong.
        motor.StopImmediately();

        actor.SetMovementSpeed(0f);
    }


    private void BeginAttack()
    {
        state = KnightState.Attack;

        attackTimer = 0f;
        attackDamageApplied = false;

        float playerDirection =
            Mathf.Sign(
                player.position.x -
                transform.position.x
            );

        motor.Face(playerDirection);
        motor.StopImmediately();

        actor.SetMovementSpeed(0f);
        actor.PlayAttack();
    }

    private void UpdateAttack()
    {
        motor.Stop();

        attackTimer += Time.deltaTime;

        // Phuong an du phong neu Animation Event khong duoc goi.
        if (
            !attackDamageApplied &&
            attackTimer >= attackWindup
        )
        {
            ApplyAttackDamage();
            attackDamageApplied = true;
        }

        float totalAttackTime =
            attackWindup +
            attackRecovery;

        if (attackTimer >= totalAttackTime)
        {
            cooldownTimer = attackCooldown;
            state = KnightState.Chase;
        }
    }

    private void HandleAttackHit()
    {
        // Chi nhan event khi Knight dang trong trang thai Attack.
        if (state != KnightState.Attack)
        {
            return;
        }

        // Khong cho phep gay sat thuong hai lan.
        if (attackDamageApplied)
        {
            return;
        }

        ApplyAttackDamage();
        attackDamageApplied = true;
    }

    private void ApplyAttackDamage()
    {
        Vector3 attackCenter =
            transform.position +
            Vector3.right *
            motor.FacingDirection *
            (attackRange * 0.5f) +
            Vector3.up *
            attackHeight;

        Collider[] hits =
            Physics.OverlapSphere(
                attackCenter,
                attackRadius,
                playerMask,
                QueryTriggerInteraction.Ignore
            );

        foreach (Collider hit in hits)
        {
            if (
                player != null &&
                hit.transform.root != player.root
            )
            {
                continue;
            }

            IDamageable damageable =
                hit.GetComponentInParent<
                    IDamageable
                >();

            if (damageable != null)
            {
                damageable.TakeDamage(
                    attackDamage
                );

                break;
            }
        }
    }

    private bool CanDetectPlayer(float range)
    {
        if (player == null)
        {
            return false;
        }

        Vector3 difference =
            player.position -
            transform.position;

        if (
            Mathf.Abs(difference.x) > range ||
            Mathf.Abs(difference.y) > verticalTolerance
        )
        {
            return false;
        }

        return sensors.CanSeeTarget(
            player,
            range
        );
    }

    private void FindPlayerIfNecessary()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float facing =
            motor == null
                ? 1f
                : motor.FacingDirection;

        Vector3 attackCenter =
            transform.position +
            Vector3.right *
            facing *
            (attackRange * 0.5f) +
            Vector3.up *
            attackHeight;

        Gizmos.DrawWireSphere(
            attackCenter,
            attackRadius
        );

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );
    }
}