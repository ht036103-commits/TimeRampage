using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GroundEnemyMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;
    [SerializeField, Min(0f)] private float acceleration = 18f;
    [SerializeField, Min(0f)] private float rotationSpeed = 12f;

    [Header("Ground")]
    [SerializeField] private bool keepGroundVelocity = true;

    public float MoveSpeed => moveSpeed;
    public float CurrentSpeedNormalized { get; private set; }

    private Rigidbody body;
    private Vector3 desiredDirection;
    private Vector3 currentHorizontalVelocity;
    private bool movementEnabled = true;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        body.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Move(Vector3 worldDirection)
    {
        if (!movementEnabled)
        {
            desiredDirection = Vector3.zero;
            return;
        }

        worldDirection.y = 0f;

        desiredDirection =
            worldDirection.sqrMagnitude > 1f
                ? worldDirection.normalized
                : worldDirection;
    }

    public void Stop()
    {
        desiredDirection = Vector3.zero;
    }

    public void SetMovementEnabled(bool value)
    {
        movementEnabled = value;

        if (!movementEnabled)
        {
            Stop();
        }
    }

    public void FaceDirection(Vector3 worldDirection)
    {
        worldDirection.y = 0f;

        if (worldDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(worldDirection.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity =
            desiredDirection.normalized * moveSpeed;

        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );

        float verticalVelocity =
            keepGroundVelocity ? body.velocity.y : 0f;

        body.velocity = new Vector3(
            currentHorizontalVelocity.x,
            verticalVelocity,
            currentHorizontalVelocity.z
        );

        CurrentSpeedNormalized =
            moveSpeed <= 0f
                ? 0f
                : currentHorizontalVelocity.magnitude / moveSpeed;

        if (desiredDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    desiredDirection.normalized
                );

            body.MoveRotation(
                Quaternion.Slerp(
                    body.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                )
            );
        }
    }

    private void OnDisable()
    {
        desiredDirection = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;

        if (body != null && !body.isKinematic)
        {
            body.velocity = new Vector3(
                0f,
                body.velocity.y,
                0f
            );
        }

        CurrentSpeedNormalized = 0f;
    }
}