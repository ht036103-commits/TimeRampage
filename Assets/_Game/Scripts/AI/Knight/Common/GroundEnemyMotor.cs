using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class GroundEnemyMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 3.5f;

    [SerializeField, Min(0f)]
    private float acceleration = 18f;

    [SerializeField, Min(0f)]
    private float rotationSpeed = 12f;

    [Header("Ground")]
    [SerializeField]
    private bool keepGroundVelocity = true;

    public float MoveSpeed => moveSpeed;

    public float CurrentSpeedNormalized
    {
        get;
        private set;
    }

    public float FacingDirection =>
        facingDirection;

    private Rigidbody body;

    private Vector3 desiredDirection;
    private Vector3 currentHorizontalVelocity;

    private float desiredSpeed;
    private float facingDirection = 1f;

    private bool movementEnabled = true;
    private bool hasFacingRequest;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        body.constraints |=
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        body.interpolation =
            RigidbodyInterpolation.Interpolate;

        desiredSpeed = moveSpeed;
    }

    public void Move(
        float direction,
        float speed
    )
    {
        if (!movementEnabled)
        {
            desiredDirection = Vector3.zero;
            return;
        }

        if (Mathf.Abs(direction) < 0.001f)
        {
            Stop();
            return;
        }

        desiredDirection =
            Vector3.right *
            Mathf.Sign(direction);

        desiredSpeed = Mathf.Max(0f, speed);
    }

    public void Move(
        Vector3 worldDirection
    )
    {
        if (!movementEnabled)
        {
            desiredDirection = Vector3.zero;
            return;
        }

        worldDirection.y = 0f;
        worldDirection.z = 0f;

        if (worldDirection.sqrMagnitude <
            0.001f)
        {
            Stop();
            return;
        }

        desiredDirection =
            worldDirection.normalized;

        desiredSpeed = moveSpeed;
    }

    public void Face(float direction)
    {
        if (Mathf.Abs(direction) < 0.001f)
        {
            return;
        }

        facingDirection =
            Mathf.Sign(direction);

        hasFacingRequest = true;
    }

    public void FaceDirection(
        Vector3 worldDirection
    )
    {
        Face(worldDirection.x);
    }

    public void Stop()
    {
        desiredDirection = Vector3.zero;
    }

    public void StopImmediately()
    {
        desiredDirection = Vector3.zero;
        currentHorizontalVelocity =
            Vector3.zero;

        if (body == null ||
            body.isKinematic)
        {
            return;
        }

        body.velocity = new Vector3(
            0f,
            keepGroundVelocity
                ? body.velocity.y
                : 0f,
            0f
        );

        CurrentSpeedNormalized = 0f;
    }

    public void SetMovementEnabled(
        bool value
    )
    {
        movementEnabled = value;

        if (!movementEnabled)
        {
            StopImmediately();
        }
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateFacing();
    }

    private void UpdateMovement()
    {
        Vector3 targetVelocity =
            desiredDirection *
            desiredSpeed;

        currentHorizontalVelocity =
            Vector3.MoveTowards(
                currentHorizontalVelocity,
                targetVelocity,
                acceleration *
                Time.fixedDeltaTime
            );

        float verticalVelocity =
            keepGroundVelocity
                ? body.velocity.y
                : 0f;

        body.velocity = new Vector3(
            currentHorizontalVelocity.x,
            verticalVelocity,
            0f
        );

        float normalizationSpeed =
            Mathf.Max(
                moveSpeed,
                desiredSpeed,
                0.01f
            );

        CurrentSpeedNormalized =
            Mathf.Clamp01(
                Mathf.Abs(
                    currentHorizontalVelocity.x
                ) /
                normalizationSpeed
            );
    }

    private void UpdateFacing()
    {
        if (!hasFacingRequest)
        {
            return;
        }

        Quaternion targetRotation =
            facingDirection > 0f
                ? Quaternion.identity
                : Quaternion.Euler(
                    0f,
                    180f,
                    0f
                );

        Quaternion nextRotation =
            Quaternion.Slerp(
                body.rotation,
                targetRotation,
                rotationSpeed *
                Time.fixedDeltaTime
            );

        body.MoveRotation(nextRotation);
    }

    private void OnDisable()
    {
        desiredDirection = Vector3.zero;
        currentHorizontalVelocity =
            Vector3.zero;

        if (body != null &&
            !body.isKinematic)
        {
            body.velocity = new Vector3(
                0f,
                keepGroundVelocity
                    ? body.velocity.y
                    : 0f,
                0f
            );
        }

        CurrentSpeedNormalized = 0f;
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(
            0f,
            moveSpeed
        );

        acceleration = Mathf.Max(
            0f,
            acceleration
        );

        rotationSpeed = Mathf.Max(
            0f,
            rotationSpeed
        );
    }
}