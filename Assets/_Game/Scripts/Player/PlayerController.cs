using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerMovementConfig config;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.22f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody body;
    private GameInputActions inputActions;

    private float moveInput;
    private float facingDirection = 1f;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private float dashTimer;
    private float dashCooldownTimer;

    private int remainingAirJumps;
    private int remainingAirDashes;

    private bool isGrounded;
    private bool jumpHeld;

    public bool IsGrounded => isGrounded;
    public bool IsDashing => dashTimer > 0f;
    public float MoveAmount => Mathf.Abs(moveInput);
    public float CurrentSpeedNormalized =>
        body == null || config == null || config.maxRunSpeed <= 0f
            ? 0f
            : Mathf.Clamp01(Mathf.Abs(body.velocity.x) / config.maxRunSpeed);
    public float FacingDirection => facingDirection;
    public float VerticalSpeed => body == null ? 0f : body.velocity.y;
    public event Action Jumped;
    public event Action DoubleJumped;
    public event Action DashStarted;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;

        inputActions = new GameInputActions();

        if (config == null)
        {
            Debug.LogError(
                "PlayerController chưa được gán PlayerMovementConfig.",
                this
            );

            enabled = false;
        }

        if (groundCheck == null)
        {
            Debug.LogError(
                "PlayerController chưa được gán GroundCheck.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    private void Update()
    {
        ReadInput();
        CheckGround();
        UpdateJumpTimers();
        UpdateDashTimers();

        if (inputActions.Player.Dash.WasPressedThisFrame())
        {
            TryStartDash();
        }
    }

    private void FixedUpdate()
    {
        if (IsDashing)
        {
            PerformDash();
            return;
        }

        MoveHorizontally();
        TryPerformJump();
        ApplyCustomGravity();
    }

    private void ReadInput()
    {
        Vector2 movement =
            inputActions.Player.Move.ReadValue<Vector2>();

        moveInput = movement.x;
        jumpHeld = inputActions.Player.Jump.IsPressed();

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            facingDirection = Mathf.Sign(moveInput);
        }

        if (inputActions.Player.Jump.WasPressedThisFrame())
        {
            jumpBufferTimer = config.jumpBufferTime;
        }
    }

    private void CheckGround()
    {
        bool touchingGround = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        // Khi đang bay lên, GroundCheck có thể vẫn chạm đất
        // trong một vài frame. Kiểm tra vận tốc sẽ tránh nhận
        // nhầm nhân vật vẫn còn đứng trên mặt đất.
        isGrounded =
            touchingGround &&
            body.velocity.y <= 0.1f;

        if (isGrounded)
        {
            remainingAirJumps = config.extraAirJumps;
            remainingAirDashes = config.airDashes;
        }
    }

    private void UpdateJumpTimers()
    {
        if (isGrounded)
        {
            coyoteTimer = config.coyoteTime;
        }
        else
        {
            coyoteTimer = Mathf.Max(
                0f,
                coyoteTimer - Time.deltaTime
            );
        }

        jumpBufferTimer = Mathf.Max(
            0f,
            jumpBufferTimer - Time.deltaTime
        );
    }

    private void UpdateDashTimers()
    {
        dashCooldownTimer = Mathf.Max(
            0f,
            dashCooldownTimer - Time.deltaTime
        );
    }

    private void MoveHorizontally()
    {
        bool hasMovementInput =
            Mathf.Abs(moveInput) > 0.01f;

        float acceleration;

        if (isGrounded)
        {
            acceleration = hasMovementInput
                ? config.groundAcceleration
                : config.groundDeceleration;
        }
        else
        {
            acceleration = hasMovementInput
                ? config.airAcceleration
                : config.airDeceleration;
        }

        float targetSpeed =
            moveInput * config.maxRunSpeed;

        Vector3 velocity = body.velocity;

        velocity.x = Mathf.MoveTowards(
            velocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime
        );

        velocity.z = 0f;
        body.velocity = velocity;
    }

    private void TryPerformJump()
    {
        if (jumpBufferTimer <= 0f)
        {
            return;
        }

        // Nhảy lần đầu hoặc coyote jump.
        if (coyoteTimer > 0f)
        {
            PerformJump(config.jumpHeight);
            Jumped?.Invoke();

            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            return;
        }

        // Double jump.
        if (remainingAirJumps > 0)
        {
            remainingAirJumps--;

            PerformJump(config.doubleJumpHeight);
            DoubleJumped?.Invoke();

            jumpBufferTimer = 0f;
        }
    }

    private void PerformJump(float height)
    {
        float jumpVelocity = Mathf.Sqrt(
            2f * config.upwardGravity * height
        );

        Vector3 velocity = body.velocity;
        velocity.y = jumpVelocity;

        body.velocity = velocity;
        isGrounded = false;
    }

    private void ApplyCustomGravity()
    {
        Vector3 velocity = body.velocity;

        if (isGrounded && velocity.y <= 0f)
        {
            // Một lực nhỏ hướng xuống giúp Player bám mặt đất.
            velocity.y = -2f;
            body.velocity = velocity;
            return;
        }

        float gravity;

        if (velocity.y < 0f)
        {
            // Rơi nhanh hơn bay lên để tránh cảm giác lơ lửng.
            gravity = config.fallingGravity;
        }
        else if (!jumpHeld)
        {
            // Thả nút sớm tạo cú nhảy thấp.
            gravity =
                config.upwardGravity *
                config.jumpCutGravityMultiplier;
        }
        else if (
            Mathf.Abs(velocity.y) <
            config.apexVelocityThreshold
        )
        {
            // Gần đỉnh cú nhảy, giảm trọng lực một chút.
            gravity =
                config.upwardGravity *
                config.apexGravityMultiplier;
        }
        else
        {
            gravity = config.upwardGravity;
        }

        velocity.y -= gravity * Time.fixedDeltaTime;

        velocity.y = Mathf.Max(
            velocity.y,
            -config.maximumFallSpeed
        );

        body.velocity = velocity;
    }

    private void TryStartDash()
    {
        if (dashCooldownTimer > 0f || IsDashing)
        {
            return;
        }

        bool canDash =
            isGrounded ||
            remainingAirDashes > 0;

        if (!canDash)
        {
            return;
        }

        if (!isGrounded)
        {
            remainingAirDashes--;
        }

        dashTimer = config.dashDuration;
        dashCooldownTimer = config.dashCooldown;

        DashStarted?.Invoke();
    }

    private void PerformDash()
    {
        dashTimer = Mathf.Max(
            0f,
            dashTimer - Time.fixedDeltaTime
        );

        body.velocity = new Vector3(
            facingDirection * config.dashSpeed,
            0f,
            0f
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}