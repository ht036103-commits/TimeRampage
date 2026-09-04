using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class TargetDummy : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 1.5f;

    private Health health;
    private Renderer[] targetRenderers;
    private Collider[] targetColliders;
    private Coroutine respawnRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();

        targetRenderers =
            GetComponentsInChildren<Renderer>();

        targetColliders =
            GetComponentsInChildren<Collider>();
    }

    private void OnEnable()
    {
        health.Died += HandleDied;
    }

    private void OnDisable()
    {
        health.Died -= HandleDied;
    }

    private void HandleDied()
    {
        if (respawnRoutine == null)
        {
            respawnRoutine =
                StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        SetVisibleAndCollidable(false);

        yield return new WaitForSeconds(respawnDelay);

        transform.position = transform.position;
        health.ResetHealth();

        SetVisibleAndCollidable(true);
        respawnRoutine = null;
    }

    private void SetVisibleAndCollidable(bool value)
    {
        foreach (Renderer targetRenderer in targetRenderers)
        {
            targetRenderer.enabled = value;
        }

        foreach (Collider targetCollider in targetColliders)
        {
            targetCollider.enabled = value;
        }
    }
}