using UnityEngine;

public class EnemySensors : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

    [Header("Environment")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckForward = 0.7f;
    [SerializeField] private float groundCheckDown = 1.2f;
    [SerializeField] private float wallCheckDistance = 0.6f;

    [Header("Detection")]
    [SerializeField, Min(0f)] private float detectionRange = 12f;
    [SerializeField, Min(0f)] private float loseTargetRange = 16f;
    [SerializeField] private bool requireLineOfSight = true;

    [Header("Line Of Sight")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float targetHeightOffset = 1f;

    public Transform Target => target;
    public bool HasTarget { get; private set; }
    public bool IsTargetVisible { get; private set; }

    public float DistanceToTarget =>
        target != null ? Vector3.Distance(transform.position, target.position) : float.MaxValue;

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
                : transform.position + Vector3.up * 1.5f;

        Vector3 targetPoint =
            targetToCheck.position + Vector3.up * targetHeightOffset;

        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;

        if (distance > maximumRange)
        {
            return false;
        }

        if (!requireLineOfSight)
        {
            return true;
        }

        bool blocked = Physics.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }

    public Vector3 DirectionToTarget
    {
        get
        {
            if (target == null)
            {
                return Vector3.zero;
            }

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
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

        float distance = DistanceToTarget;

        if (!HasTarget)
        {
            HasTarget = distance <= detectionRange;
        }
        else if (distance > loseTargetRange)
        {
            HasTarget = false;
        }

        IsTargetVisible =
            HasTarget &&
            (!requireLineOfSight || CheckLineOfSight());
    }

    public bool HasGroundAhead(float direction)
    {
        Vector3 origin =
            groundCheck != null
                ? groundCheck.position
                : transform.position;

        origin += Vector3.right *
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

    public bool HasWallAhead(float direction)
    {
        Vector3 origin =
            eyePoint != null
                ? eyePoint.position
                : transform.position + Vector3.up * 0.8f;

        return Physics.Raycast(
            origin,
            Vector3.right * Mathf.Sign(direction),
            wallCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        HasTarget = false;
        IsTargetVisible = false;
    }

    private void FindTargetIfMissing()
    {
        GameObject targetObject =
            GameObject.FindGameObjectWithTag(targetTag);

        if (targetObject != null)
        {
            target = targetObject.transform;
        }
    }

    private bool CheckLineOfSight()
    {
        Vector3 origin =
            eyePoint != null
                ? eyePoint.position
                : transform.position + Vector3.up * 1.5f;

        Vector3 targetPoint =
            target.position + Vector3.up * targetHeightOffset;

        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
        {
            return true;
        }

        bool blocked = Physics.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }

    private void OnValidate()
    {
        detectionRange = Mathf.Max(0f, detectionRange);
        loseTargetRange = Mathf.Max(detectionRange, loseTargetRange);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);

        if (target == null)
        {
            return;
        }

        Vector3 origin =
            eyePoint != null
                ? eyePoint.position
                : transform.position + Vector3.up * 1.5f;

        Vector3 targetPoint =
            target.position + Vector3.up * targetHeightOffset;

        Gizmos.color = IsTargetVisible ? Color.green : Color.red;
        Gizmos.DrawLine(origin, targetPoint);
    }
}