using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public sealed class Projectile : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private LayerMask hitMask;

    private Rigidbody body;
    private Transform ownerRoot;

    private int damage;
    private float lifeTimer;
    private bool isLaunched;

    private Action<Projectile> releaseAction;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Launch(
        Transform newOwnerRoot,
        Vector3 position,
        Vector3 direction,
        int newDamage,
        float speed,
        float lifetime,
        Action<Projectile> onRelease
    )
    {
        ownerRoot = newOwnerRoot;
        damage = newDamage;
        lifeTimer = lifetime;
        releaseAction = onRelease;
        isLaunched = true;

        transform.SetPositionAndRotation(
            position,
            Quaternion.LookRotation(
                Vector3.forward,
                direction
            )
        );

        body.velocity =
            direction.normalized * speed;

        body.angularVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (!isLaunched)
        {
            return;
        }

        lifeTimer -= Time.deltaTime;

        if (lifeTimer <= 0f)
        {
            Release();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLaunched)
        {
            return;
        }

        if (
            ownerRoot != null &&
            other.transform.root == ownerRoot
        )
        {
            return;
        }

        int otherLayerMask =
            1 << other.gameObject.layer;

        if ((hitMask.value & otherLayerMask) == 0)
        {
            return;
        }

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        damageable?.TakeDamage(damage);

        Release();
    }

    private void Release()
    {
        if (!isLaunched)
        {
            return;
        }

        isLaunched = false;

        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        ownerRoot = null;
        damage = 0;
        lifeTimer = 0f;

        Action<Projectile> callback = releaseAction;
        releaseAction = null;

        callback?.Invoke(this);
    }
}