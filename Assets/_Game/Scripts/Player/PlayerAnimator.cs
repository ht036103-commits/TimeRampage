using UnityEngine;

public sealed class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;

    [Header("Facing Direction")]
    [SerializeField] private float rightFacingY = 90f;
    [SerializeField] private float leftFacingY = -90f;
    [SerializeField] private float rotationSpeed = 20f;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int VerticalSpeedHash =
        Animator.StringToHash("VerticalSpeed");

    private static readonly int IsGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int IsDashingHash =
        Animator.StringToHash("IsDashing");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    private static readonly int DoubleJumpHash =
        Animator.StringToHash("DoubleJump");

    private static readonly int DashHash =
        Animator.StringToHash("Dash");

    private static readonly int ShootHash =
        Animator.StringToHash("Shoot");

    private static readonly int HurtHash =
        Animator.StringToHash("Hurt");

    private static readonly int DeadHash =
        Animator.StringToHash("Dead");

    private bool CanUseAnimator =>
        animator != null &&
        animator.isActiveAndEnabled &&
        animator.runtimeAnimatorController != null;

    private void OnEnable()
    {
        if (controller == null)
        {
            return;
        }

        controller.Jumped += HandleJump;
        controller.DoubleJumped += HandleDoubleJump;
        controller.DashStarted += HandleDash;
    }

    private void OnDisable()
    {
        if (controller == null)
        {
            return;
        }

        controller.Jumped -= HandleJump;
        controller.DoubleJumped -= HandleDoubleJump;
        controller.DashStarted -= HandleDash;
    }

    private void Update()
    {
        if (controller == null ||
            visual == null ||
            !CanUseAnimator)
        {
            return;
        }

        UpdateAnimatorParameters();
        UpdateFacingDirection();
    }

    private void UpdateAnimatorParameters()
    {
        if (!CanUseAnimator)
        {
            return;
        }

        animator.SetFloat(
            SpeedHash,
            controller.CurrentSpeedNormalized,
            0.1f,
            Time.deltaTime
        );

        animator.SetFloat(
            VerticalSpeedHash,
            controller.VerticalSpeed
        );

        animator.SetBool(
            IsGroundedHash,
            controller.IsGrounded
        );

        animator.SetBool(
            IsDashingHash,
            controller.IsDashing
        );
    }

    private void UpdateFacingDirection()
    {
        if (!CanUseAnimator)
        {
            return;
        }

        float targetY = controller.FacingDirection > 0f
            ? rightFacingY
            : leftFacingY;

        Quaternion targetRotation =
            Quaternion.Euler(0f, targetY, 0f);

        visual.localRotation = Quaternion.Slerp(
            visual.localRotation,
            targetRotation,
            1f - Mathf.Exp(
                -rotationSpeed * Time.deltaTime
            )
        );
    }

    private void HandleJump()
    {
        if (CanUseAnimator)
        {
            animator.SetTrigger(JumpHash);
        }
    }

    private void HandleDoubleJump()
    {
        if (CanUseAnimator)
        {
            animator.SetTrigger(DoubleJumpHash);
        }
    }

    private void HandleDash()
    {
        if (CanUseAnimator)
        {
            animator.SetTrigger(DashHash);
        }
    }

    public void PlayShoot()
    {
        if (CanUseAnimator)
        {
            animator.SetTrigger(ShootHash);
        }
    }

    public void PlayHurt()
    {
        if (CanUseAnimator)
        {
            animator.SetTrigger(HurtHash);
        }
    }

    public void SetDead(bool value)
    {
        if (CanUseAnimator)
        {
            animator.SetBool(DeadHash, value);
        }
    }
}