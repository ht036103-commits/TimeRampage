using UnityEngine;
using UnityEngine.InputSystem;

public sealed class WeaponController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private WeaponConfig config;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera gameCamera;

    private GameInputActions inputActions;

    private Vector3 aimDirection = Vector3.right;
    private float nextFireTime;
    private float scatterEndTime;

    public bool IsScatterActive =>
        Time.time < scatterEndTime;

    public float ScatterTimeRemaining =>
        Mathf.Max(0f, scatterEndTime - Time.time);

    private void Awake()
    {
        inputActions = new GameInputActions();

        if (gameCamera == null)
        {
            gameCamera = Camera.main;
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
        UpdateAimDirection();
        UpdateWeaponPivot();

        if (
            inputActions.Player.Shoot.IsPressed() &&
            Time.time >= nextFireTime
        )
        {
            Fire();
        }
    }

    private void UpdateAimDirection()
    {
        Vector2 rawAim =
            inputActions.Player.Aim.ReadValue<Vector2>();

        bool mouseIsControllingAim =
            inputActions.Player.Aim.activeControl != null &&
            inputActions.Player.Aim.activeControl.device
                is Mouse;

        if (mouseIsControllingAim)
        {
            UpdateMouseAim(rawAim);
            return;
        }

        if (rawAim.sqrMagnitude > 0.04f)
        {
            aimDirection = new Vector3(
                rawAim.x,
                rawAim.y,
                0f
            ).normalized;

            return;
        }

        aimDirection = new Vector3(
            playerController.FacingDirection,
            0f,
            0f
        );
    }

    private void UpdateMouseAim(
        Vector2 mouseScreenPosition
    )
    {
        if (gameCamera == null)
        {
            return;
        }

        Ray mouseRay =
            gameCamera.ScreenPointToRay(
                mouseScreenPosition
            );

        Plane gameplayPlane = new Plane(
            Vector3.forward,
            weaponPivot.position
        );

        if (
            gameplayPlane.Raycast(
                mouseRay,
                out float distance
            )
        )
        {
            Vector3 mouseWorldPosition =
                mouseRay.GetPoint(distance);

            Vector3 direction =
                mouseWorldPosition -
                weaponPivot.position;

            direction.z = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                aimDirection = direction.normalized;
            }
        }
    }

    private void UpdateWeaponPivot()
    {
        float angle = Mathf.Atan2(
            aimDirection.y,
            aimDirection.x
        ) * Mathf.Rad2Deg;

        weaponPivot.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    private void Fire()
    {
        float fireInterval =
            1f / config.shotsPerSecond;

        nextFireTime =
            Time.time + fireInterval;

        if (IsScatterActive)
        {
            FireScatter();
        }
        else
        {
            FireSingleProjectile(aimDirection);
        }

        if (playerAnimator != null)
        {
            playerAnimator.PlayShoot();
        }
    }

    private void FireScatter()
    {
        int count =
            config.scatterProjectileCount;

        float totalAngle =
            config.scatterStepAngle *
            (count - 1);

        float startingAngle =
            -totalAngle * 0.5f;

        for (int index = 0; index < count; index++)
        {
            float angle =
                startingAngle +
                index * config.scatterStepAngle;

            Vector3 scatteredDirection =
                Quaternion.AngleAxis(
                    angle,
                    Vector3.forward
                ) * aimDirection;

            FireSingleProjectile(
                scatteredDirection
            );
        }
    }

    private void FireSingleProjectile(
        Vector3 direction
    )
    {
        Projectile projectile =
            projectilePool.Get();

        projectile.Launch(
            playerController.transform,
            firePoint.position,
            direction.normalized,
            config.damage,
            config.projectileSpeed,
            config.projectileLifetime,
            projectilePool.Release
        );
    }

    public void ActivateScatter(float duration)
    {
        float startTime = Mathf.Max(
            Time.time,
            scatterEndTime
        );

        scatterEndTime =
            startTime + duration;
    }
}