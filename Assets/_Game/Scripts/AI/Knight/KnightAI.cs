using UnityEngine;

[RequireComponent(typeof(EnemyActor))]
[RequireComponent(typeof(EnemySensors))]
[RequireComponent(typeof(GroundEnemyMotor))]
public sealed class KnightAI : MonoBehaviour
{
    private enum KnightState
    {
        Idle,
        Chase,
        Dead
    }

    [Header("References")]
    [SerializeField] private EnemyActor actor;
    [SerializeField] private EnemySensors sensors;
    [SerializeField] private GroundEnemyMotor motor;

    private KnightState currentState;

    private void Reset()
    {
        actor = GetComponent<EnemyActor>();
        sensors = GetComponent<EnemySensors>();
        motor = GetComponent<GroundEnemyMotor>();
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
    }

    private void Update()
    {
        if (actor.IsDead)
        {
            ChangeState(KnightState.Dead);
        }
        else if (sensors.HasTarget &&
                 sensors.CanSeeTarget)
        {
            ChangeState(KnightState.Chase);
        }
        else
        {
            ChangeState(KnightState.Idle);
        }

        UpdateCurrentState();

        actor.SetMovementSpeed(
            motor.CurrentSpeedNormalized
        );
    }

    private void ChangeState(KnightState newState)
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
                motor.Stop();
                break;

            case KnightState.Chase:
                motor.Move(
                    sensors.DirectionToTarget
                );
                break;

            case KnightState.Dead:
                motor.Stop();
                enabled = false;
                break;
        }
    }

    private void OnDisable()
    {
        if (motor != null)
        {
            motor.Stop();
        }
    }
}