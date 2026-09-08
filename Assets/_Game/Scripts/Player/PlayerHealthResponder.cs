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
        {
            health = GetComponent<Health>();
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }

        initialPosition = transform.position;
    }

    private void OnEnable()
    {
        health.Damaged += HandleDamaged;
        health.Died += HandleDied;
    }

    private void OnDisable()
    {
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