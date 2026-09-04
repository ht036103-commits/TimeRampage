using UnityEngine;

public class EnemySensors : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

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
    public bool CanSeeTarget { get; private set; }

    public float DistanceToTarget
    {
        get
        {
            if (target == null)
            {
                return Mathf.Infinity;
            }

            Vector3 offset =
                target.position - transform.position;

            offset.y = 0f;
            return offset.magnitude;
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
                target.position - transform.position;

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
            CanSeeTarget = false;
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

        CanSeeTarget =
            HasTarget &&
            (!requireLineOfSight || CheckLineOfSight());
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        HasTarget = false;
        CanSeeTarget = false;
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
        loseTargetRange =
            Mathf.Max(detectionRange, loseTargetRange);
    }

    private void OnDrawGizmosSelected()
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

        Gizmos.color =
            CanSeeTarget ? Color.green : Color.red;

        Gizmos.DrawLine(origin, targetPoint);
    }
}