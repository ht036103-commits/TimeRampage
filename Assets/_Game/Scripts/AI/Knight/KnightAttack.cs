using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyActor))]
public sealed class KnightAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyActor actor;
    [SerializeField] private Transform attackPoint;

    [Header("Attack")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField, Min(0.1f)] private float attackRadius = 1.1f;
    [SerializeField, Min(1)] private int damage = 15;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float hitDelay = 0.35f;
    [SerializeField, Min(0.1f)] private float attackDuration = 0.8f;
    [SerializeField, Min(0.1f)] private float cooldown = 1.2f;

    private readonly Collider[] hitResults =
        new Collider[8];

    private Coroutine attackRoutine;
    private float nextAttackTime;
    private bool isAttacking;

    public bool IsAttacking => isAttacking;

    public bool CanAttack =>
        !isAttacking &&
        Time.time >= nextAttackTime &&
        actor != null &&
        !actor.IsDead;

    private void Reset()
    {
        actor = GetComponent<EnemyActor>();
    }

    private void Awake()
    {
        if (actor == null)
        {
            actor = GetComponent<EnemyActor>();
        }
    }

    public bool TryAttack()
    {
        if (!CanAttack || attackPoint == null)
        {
            return false;
        }

        attackRoutine =
            StartCoroutine(AttackRoutine());

        return true;
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + cooldown;

        actor.PlayAttack();

        yield return new WaitForSeconds(hitDelay);

        if (!actor.IsDead)
        {
            DealDamage();
        }

        float remainingDuration =
            Mathf.Max(0f, attackDuration - hitDelay);

        if (remainingDuration > 0f)
        {
            yield return new WaitForSeconds(
                remainingDuration
            );
        }

        isAttacking = false;
        attackRoutine = null;
    }

    private void DealDamage()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            attackPoint.position,
            attackRadius,
            hitResults,
            targetMask,
            QueryTriggerInteraction.Ignore
        );

        for (int index = 0;
             index < hitCount;
             index++)
        {
            Collider hitCollider =
                hitResults[index];

            if (hitCollider == null)
            {
                continue;
            }

            IDamageable damageable =
                hitCollider.GetComponentInParent
                    <IDamageable>();

            if (damageable == null)
            {
                continue;
            }

            damageable.TakeDamage(damage);

            // Mỗi nhát chém chỉ đánh một mục tiêu.
            break;
        }

        for (int index = 0;
             index < hitCount;
             index++)
        {
            hitResults[index] = null;
        }
    }

    private void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;
    }

    private void OnValidate()
    {
        attackRadius = Mathf.Max(
            0.1f,
            attackRadius
        );

        damage = Mathf.Max(1, damage);
        hitDelay = Mathf.Max(0f, hitDelay);

        attackDuration = Mathf.Max(
            hitDelay,
            attackDuration
        );

        cooldown = Mathf.Max(
            attackDuration,
            cooldown
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRadius
        );
    }
}