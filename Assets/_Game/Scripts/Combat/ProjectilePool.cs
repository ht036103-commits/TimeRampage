using UnityEngine;
using UnityEngine.Pool;

public sealed class ProjectilePool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int defaultCapacity = 32;
    [SerializeField] private int maximumSize = 128;

    private ObjectPool<Projectile> pool;

    private void Awake()
    {
        pool = new ObjectPool<Projectile>(
            createFunc: CreateProjectile,
            actionOnGet: HandleGetProjectile,
            actionOnRelease: HandleReleaseProjectile,
            actionOnDestroy: DestroyProjectile,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maximumSize
        );
    }

    public Projectile Get()
    {
        return pool.Get();
    }

    public void Release(Projectile projectile)
    {
        pool.Release(projectile);
    }

    private Projectile CreateProjectile()
    {
        Projectile projectile = Instantiate(
            projectilePrefab,
            transform
        );

        projectile.gameObject.SetActive(false);
        return projectile;
    }

    private void HandleGetProjectile(
        Projectile projectile
    )
    {
        projectile.gameObject.SetActive(true);
    }

    private void HandleReleaseProjectile(
        Projectile projectile
    )
    {
        projectile.gameObject.SetActive(false);
    }

    private void DestroyProjectile(
        Projectile projectile
    )
    {
        Destroy(projectile.gameObject);
    }
}