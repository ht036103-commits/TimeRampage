using UnityEngine;

[RequireComponent(typeof(EnemyActor))]
[RequireComponent(typeof(EnemySensors))]
[RequireComponent(typeof(GroundEnemyMotor))]
[RequireComponent(typeof(KnightAttack))]
public sealed class KnightAI : MonoBehaviour
{
    private enum KnightState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("References")]
    [SerializeField] private EnemyActor actor;
    [SerializeField] private EnemySensors sensors;
    [SerializeField] private GroundEnemyMotor motor;
    [SerializeField] private KnightAttack attack;

    [Header("Combat")]
    [SerializeField, Min(0.1f)]
    private float attackRange = 1.7f;

    private KnightState currentState;

    private void Reset()
    {
        actor = GetComponent<EnemyActor>();
        sensors = GetComponent<EnemySensors>();
        motor = GetComponent<GroundEnemyMotor>();
        attack = GetComponent<KnightAttack>();
    }

    private void Awake()
    {
        if (actor == null)
        {
            actor = GetComponent<EnemyActor>();
        }

        if (sensors == null)
        {
            sensors = GetComponent<EnemySensors>();
        }

        if (motor == null)
        {
            motor = GetComponent<GroundEnemyMotor>();
        }

        if (attack == null)
        {
            attack = GetComponent<KnightAttack>();
        }
    }

    private void Update()
    {
        SelectState();
        UpdateCurrentState();

        actor.SetMovementSpeed(
            motor.CurrentSpeedNormalized
        );
    }

    private void SelectState()
    {
        if (actor.IsDead)
        {
            ChangeState(KnightState.Dead);
            return;
        }

        if (!sensors.HasTarget ||
            !sensors.CanSeeTarget)
        {
            ChangeState(KnightState.Idle);
            return;
        }

        if (sensors.DistanceToTarget <=
            attackRange)
        {
            ChangeState(KnightState.Attack);
            return;
        }

        ChangeState(KnightState.Chase);
    }

    private void ChangeState(
        KnightState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
    }

    private void UpdateCurrentState()
    {
        switch (currentState)
        {
            case KnightState.Idle:
                UpdateIdleState();
                break;

            case KnightState.Chase:
                UpdateChaseState();
                break;

            case KnightState.Attack:
                UpdateAttackState();
                break;

            case KnightState.Dead:
                UpdateDeadState();
                break;
        }
    }

    private void UpdateIdleState()
    {
        motor.Stop();
    }

    private void UpdateChaseState()
    {
        motor.Move(
            sensors.DirectionToTarget
        );
    }

    private void UpdateAttackState()
    {
        motor.Stop();

        motor.FaceDirection(
            sensors.DirectionToTarget
        );

        attack.TryAttack();
    }

    private void UpdateDeadState()
    {
        motor.Stop();
        enabled = false;
    }

    private void OnDisable()
    {
        if (motor != null)
        {
            motor.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}