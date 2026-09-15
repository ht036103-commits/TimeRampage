using UnityEngine;

public sealed class ArcherAI : MonoBehaviour
{
    private enum ArcherState
    {
        Patrol,
        Engage,
        Shooting
    }

    [Header("References")]
    [SerializeField] private EnemyActor actor;
    [SerializeField] private GroundEnemyMotor motor;
    [SerializeField] private EnemySensors sensors;
    [SerializeField] private Transform player;
    [SerializeField] private ProjectilePool arrowPool;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float repositionSpeed = 3.5f;
    [SerializeField] private float verticalTolerance = 4f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float loseTargetRange = 16f;

    [Header("Preferred Distance")]
    [SerializeField] private float minimumDistance = 4.5f;
    [SerializeField] private float maximumDistance = 8f;

    [Header("Arrow")]
    [SerializeField] private Vector2 arrowSpawnOffset =
        new Vector2(0.75f, 0.4f);

    [SerializeField] private float targetHeightOffset = 0.4f;
    [SerializeField] private int arrowDamage = 10;
    [SerializeField] private float arrowSpeed = 18f;
    [SerializeField] private float arrowLifetime = 4f;

    [Header("Shooting Timing")]
    [SerializeField] private float shootingWindup = 0.5f;
    [SerializeField] private float shootingRecovery = 0.6f;
    [SerializeField] private float shootingCooldown = 1.5f;

    private ArcherState state =
        ArcherState.Patrol;

    private float patrolDirection = 1f;
    private float cooldownTimer;
    private float shootingTimer;
    private bool arrowReleased;

    private void Start()
    {
        FindPlayerIfNecessary();
    }

    private void Update()
    {
        if (actor == null || actor.IsDead)
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
            case ArcherState.Patrol:
                UpdatePatrol();
                break;

            case ArcherState.Engage:
                UpdateEngage();
                break;

            case ArcherState.Shooting:
                UpdateShooting();
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (CanSeePlayer(detectionRange))
        {
            state = ArcherState.Engage;
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
            return;
        }

        motor.Move(
            patrolDirection,
            patrolSpeed
        );

        actor.SetMovementSpeed(
            patrolSpeed /
            repositionSpeed
        );
    }

    private void UpdateEngage()
    {
        if (!CanSeePlayer(loseTargetRange))
        {
            state = ArcherState.Patrol;
            motor.Stop();
            return;
        }

        Vector3 difference =
            player.position -
            transform.position;

        float horizontalDistance =
            Mathf.Abs(difference.x);

        float directionToPlayer =
            Mathf.Sign(difference.x);

        motor.Face(directionToPlayer);

        if (horizontalDistance < minimumDistance)
        {
            float retreatDirection =
                -directionToPlayer;

            if (CanMove(retreatDirection))
            {
                motor.Move(
                    retreatDirection,
                    repositionSpeed
                );

                actor.SetMovementSpeed(1f);
            }
            else
            {
                motor.Stop();
                actor.SetMovementSpeed(0f);
            }

            return;
        }

        if (horizontalDistance > maximumDistance)
        {
            if (CanMove(directionToPlayer))
            {
                motor.Move(
                    directionToPlayer,
                    repositionSpeed
                );

                actor.SetMovementSpeed(1f);
            }
            else
            {
                motor.Stop();
                actor.SetMovementSpeed(0f);
            }

            return;
        }

        motor.Stop();
        actor.SetMovementSpeed(0f);

        if (cooldownTimer <= 0f)
        {
            BeginShooting();
        }
    }

    private void BeginShooting()
    {
        state = ArcherState.Shooting;

        shootingTimer = 0f;
        arrowReleased = false;

        motor.StopImmediately();
        actor.SetMovementSpeed(0f);
        actor.PlayAttack();
    }

    private void UpdateShooting()
    {
        motor.Stop();

        if (player != null)
        {
            motor.Face(
                player.position.x -
                transform.position.x
            );
        }

        shootingTimer += Time.deltaTime;

        if (
            !arrowReleased &&
            shootingTimer >= shootingWindup
        )
        {
            if (CanSeePlayer(loseTargetRange))
            {
                FireArrow();
            }

            arrowReleased = true;
        }

        float totalTime =
            shootingWindup +
            shootingRecovery;

        if (shootingTimer >= totalTime)
        {
            cooldownTimer = shootingCooldown;
            state = ArcherState.Engage;
        }
    }

    private void FireArrow()
    {
        float facing =
            motor.FacingDirection;

        Vector3 spawnPosition =
            transform.position +
            Vector3.right *
            facing *
            Mathf.Abs(arrowSpawnOffset.x) +
            Vector3.up *
            arrowSpawnOffset.y;

        Vector3 targetPosition =
            player.position +
            Vector3.up *
            targetHeightOffset;

        Vector3 direction =
            (targetPosition - spawnPosition)
            .normalized;

        if (arrowPool == null)
        {
            Debug.LogError(
                $"{name}: Archer chưa được gán Arrow Pool.",
                this
            );

            return;
        }

        Projectile arrow = arrowPool.Get();

        if (arrow == null)
        {
            return;
        }

        arrow.Launch(
            transform,
            spawnPosition,
            direction,
            arrowDamage,
            arrowSpeed,
            arrowLifetime,
            arrowPool.Release
        );
    }

    private bool CanMove(float direction)
    {
        return
            sensors.HasGroundAhead(direction) &&
            !sensors.HasWallAhead(direction);
    }

    private bool CanSeePlayer(float range)
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
            Mathf.Abs(difference.y) >
            verticalTolerance
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
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            minimumDistance
        );

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            maximumDistance
        );
    }
}