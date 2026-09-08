using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class PlayerHealthResponder :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Rigidbody body;
    [SerializeField] private Transform respawnPoint;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 2f;

    private Vector3 initialPosition;
    private Coroutine respawnRoutine;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (body == null)
            body = GetComponent<Rigidbody>();

        if (playerController == null)
            playerController =
                GetComponent<PlayerController>();

        if (weaponController == null)
            weaponController =
                GetComponent<WeaponController>();

        if (playerAnimator == null)
            playerAnimator =
                GetComponent<PlayerAnimator>();

        initialPosition = transform.position;

        if (health == null)
            Debug.LogError(
                "PlayerHealthResponder thiếu Health.",
                this
            );

        if (body == null)
            Debug.LogError(
                "PlayerHealthResponder thiếu Rigidbody.",
                this
            );
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

    private void HandleDamaged(int damage)
    {
        if (!health.IsDead)
        {
            playerAnimator?.PlayHurt();
        }
    }

    private void HandleDied()
    {
        playerAnimator?.SetDead(true);

        playerController.enabled = false;
        weaponController.enabled = false;

        body.velocity = Vector3.zero;

        if (respawnRoutine == null)
        {
            respawnRoutine =
                StartCoroutine(
                    RespawnRoutine()
                );
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(
            respawnDelay
        );

        Vector3 targetPosition =
            respawnPoint != null
                ? respawnPoint.position
                : initialPosition;

        body.position = targetPosition;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        health.ResetHealth();

        playerAnimator?.SetDead(false);

        playerController.enabled = true;
        weaponController.enabled = true;

        respawnRoutine = null;
    }
}