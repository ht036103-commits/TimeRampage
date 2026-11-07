using UnityEngine;

public sealed class EnemySensors :
    MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform target;

    [SerializeField]
    private string targetTag = "Player";

    [Header("Environment References")]
    [SerializeField]
    private Transform groundCheck;

    [SerializeField]
    private Transform eyePoint;

    [Header("Environment Layers")]
    [SerializeField]
    private LayerMask groundMask;

    [SerializeField]
    private LayerMask obstacleMask;

    [Header("Ground Detection")]
    [SerializeField, Min(0f)]
    private float groundCheckForward = 0.7f;

    [SerializeField, Min(0f)]
    private float groundCheckDown = 1.2f;

    [Header("Wall Detection")]
    [SerializeField, Min(0f)]
    private float wallCheckDistance = 0.6f;

    [Header("Player Detection")]
    [SerializeField, Min(0f)]
    private float detectionRange = 12f;

    [SerializeField, Min(0f)]
    private float loseTargetRange = 16f;

    [SerializeField]
    private bool requireLineOfSight = true;

    [SerializeField]
    private float targetHeightOffset = 1f;

    public Transform Target => target;

    public bool HasTarget
    {
        get;
        private set;
    }

    public bool IsTargetVisible
    {
        get;
        private set;
    }

    public float DistanceToTarget
    {
        get
        {
            if (target == null)
            {
                return Mathf.Infinity;
            }

            Vector3 difference =
                target.position -
                transform.position;

            difference.z = 0f;

            return difference.magnitude;
        }
    }

    public Vector3 DirectionToTarget
    {
        get
        {
            if (target == null)
            {
                return Vector3.zero;
            }

            Vector3 direction =
                target.position -
                transform.position;

            direction.y = 0f;
            direction.z = 0f;

            return direction.normalized;
        }
    }

    private void Start()
    {
        FindTargetIfMissing();
    }

    private void Update()
    {
        if (target == null)
        {
            FindTargetIfMissing();

            HasTarget = false;
            IsTargetVisible = false;
            return;
        }

        float distance =
            DistanceToTarget;

        if (!HasTarget)
        {
            HasTarget =
                distance <= detectionRange;
        }
        else if (
            distance > loseTargetRange
        )
        {
            HasTarget = false;
        }

        IsTargetVisible =
            HasTarget &&
            CanSeeTarget(
                target,
                loseTargetRange
            );
    }

    public bool HasGroundAhead(
        float direction
    )
    {
        if (Mathf.Abs(direction) <
            0.001f)
        {
            return true;
        }

        Vector3 origin =
            groundCheck != null
                ? groundCheck.position
                : transform.position;

        origin +=
            Vector3.right *
            Mathf.Sign(direction) *
            groundCheckForward;

        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDown,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    public bool HasWallAhead(
        float direction
    )
    {
        if (Mathf.Abs(direction) <
            0.001f)
        {
            return false;
        }

        Vector3 origin =
            eyePoint != null
                ? eyePoint.position
                : transform.position +
                  Vector3.up * 0.8f;

        return Physics.Raycast(
            origin,
            Vector3.right *
            Mathf.Sign(direction),
            wallCheckDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );
    }

    public bool CanSeeTarget(
        Transform targetToCheck,
        float maximumRange
    )
    {
        if (targetToCheck == null)
        {
            return false;
        }

        Vector3 origin =
            eyePoint != null
                ? eyePoint.position
                : transform.position +
                  Vector3.up * 1.5f;

        Vector3 targetPoint =
            targetToCheck.position +
            Vector3.up *
            targetHeightOffset;

        Vector3 direction =
            targetPoint - origin;

        direction.z = 0f;

        float distance =
            direction.magnitude;

        if (distance <= 0.001f)
        {
            return true;
        }

        if (distance > maximumRange)
        {
            return false;
        }

        if (!requireLineOfSight)
        {
            return true;
        }

        bool blocked =
            Physics.Raycast(
                origin,
                direction.normalized,
                distance,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            );

        return !blocked;
    }

    public void SetTarget(
        Transform newTarget
    )
    {
        target = newTarget;
        HasTarget = false;
        IsTargetVisible = false;
    }

    private void FindTargetIfMissing()
    {
        GameObject targetObject =
            GameObject.FindGameObjectWithTag(
                targetTag
            );

        if (targetObject != null)
        {
            target =
                targetObject.transform;
        }
    }

    private void OnValidate()
    {
        groundCheckForward =
            Mathf.Max(
                0f,
                groundCheckForward
            );

        groundCheckDown =
            Mathf.Max(
                0f,
                groundCheckDown
            );

        wallCheckDistance =
            Mathf.Max(
                0f,
                wallCheckDistance
            );

        detectionRange =
            Mathf.Max(
                0f,
                detectionRange
            );

        loseTargetRange =
            Mathf.Max(
                detectionRange,
                loseTargetRange
            );
    }

    private void OnDrawGizmosSelected()
    {
        DrawDetectionRanges();
        DrawGroundSensor();
        DrawWallSensor();
        DrawLineOfSight();
    }

    private void DrawDetectionRanges()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            loseTargetRange
        );
    }

    private void DrawGroundSensor()
    {
        Vector3 origin =
            groundCheck != null
                ? groundCheck.position
                : transform.position;

        float direction = 1f;

        GroundEnemyMotor motor =
            GetComponent<
                GroundEnemyMotor
            >();

        if (motor != null)
        {
            direction =
                motor.FacingDirection;
        }

        origin +=
            Vector3.right *
            Mathf.Sign(direction) *
            groundCheckForward;

        bool foundGround =
            Physics.Raycast(
                origin,
                Vector3.down,
                groundCheckDown,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

        Gizmos.color =
            foundGround
                ? Color.green
                : Color.red;

        Gizmos.DrawLine(
            origin,
            origin +
            Vector3.down *
            groundCheckDown
        );
    }

    private void DrawWallSensor()
    {
        Vector3 origin =
            eyePoint != null
                ? eyePoint.position
                : transform.position +
                  Vector3.up * 0.8f;

        float direction = 1f;

        GroundEnemyMotor motor =
            GetComponent<
                GroundEnemyMotor
            >();

        if (motor != null)
        {
            direction =
                motor.FacingDirection;
        }

        bool foundWall =
            Physics.Raycast(
                origin,
                Vector3.right *
                Mathf.Sign(direction),
                wallCheckDistance,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            );

        Gizmos.color =
            foundWall
                ? Color.red
                : Color.cyan;

        Gizmos.DrawLine(
            origin,
            origin +
            Vector3.right *
            Mathf.Sign(direction) *
            wallCheckDistance
        );
    }

    private void DrawLineOfSight()
    {
        if (target == null)
        {
            return;
        }

        Vector3 origin =
            eyePoint != null
                ? eyePoint.position
                : transform.position +
                  Vector3.up * 1.5f;

        Vector3 targetPoint =
            target.position +
            Vector3.up *
            targetHeightOffset;

        Gizmos.color =
            IsTargetVisible
                ? Color.green
                : Color.red;

        Gizmos.DrawLine(
            origin,
            targetPoint
        );
    }
}