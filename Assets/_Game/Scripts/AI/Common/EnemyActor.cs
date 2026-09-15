using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyActor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody body;
    [SerializeField] private Collider bodyCollider;

    [Header("Death")]
    [SerializeField] private bool disableColliderOnDeath = true;
    [SerializeField] private float destroyDelay = 4f;

    public Health Health => health;
    public bool IsDead => health != null && health.IsDead;
    public Animator Animator => animator;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int HurtHash =
        Animator.StringToHash("Hurt");

    private static readonly int DeadHash =
        Animator.StringToHash("Dead");

    private bool CanUseAnimator =>
        animator != null &&
        animator.isActiveAndEnabled &&
        animator.runtimeAnimatorController != null;

    private void Reset()
    {
        health = GetComponent<Health>();
        body = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }

        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError(
                $"{name}: EnemyActor không tìm thấy Animator.",
                this
            );
        }
        else if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError(
                $"{name}: Animator của model chưa được gán Animator Controller.",
                animator
            );
        }
    }

    private void OnEnable()
    {
        if (health == null)
        {
            return;
        }

        health.Damaged += HandleDamaged;
        health.Died += HandleDied;
    }

    private void OnDisable()
    {
        if (health == null)
        {
            return;
        }

        health.Damaged -= HandleDamaged;
        health.Died -= HandleDied;
    }

    public void SetMovementSpeed(float normalizedSpeed)
    {
        if (!CanUseAnimator || IsDead)
        {
            return;
        }

        animator.SetFloat(
            SpeedHash,
            Mathf.Clamp01(normalizedSpeed),
            0.1f,
            Time.deltaTime
        );
    }

    public void PlayAttack()
    {
        if (!CanUseAnimator || IsDead)
        {
            return;
        }

        animator.ResetTrigger(HurtHash);
        animator.SetTrigger(AttackHash);
    }

    private void HandleDamaged(int amount)
    {
        if (!CanUseAnimator || IsDead)
        {
            return;
        }

        animator.SetTrigger(HurtHash);
    }

    private void HandleDied()
    {
        if (CanUseAnimator)
        {
            animator.ResetTrigger(HurtHash);
            animator.SetBool(DeadHash, true);
        }

        GroundEnemyMotor motor =
            GetComponent<GroundEnemyMotor>();

        if (motor != null)
        {
            motor.Stop();
            motor.enabled = false;
        }

        if (body != null)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }

        if (disableColliderOnDeath &&
            bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (destroyDelay > 0f)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}