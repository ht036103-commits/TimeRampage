using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class DestructibleObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private GameObject intactVisual;
    [SerializeField] private GameObject destroyedVisual;

    [Header("Destruction")]
    [SerializeField, Min(0f)] private float destroyDelay = 2f;

    private bool isDestroyed;

    private void Reset()
    {
        health = GetComponent<Health>();
        bodyCollider = GetComponent<Collider>();
    }

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider>();
        }

        if (destroyedVisual != null)
        {
            destroyedVisual.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Died += HandleDestroyed;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= HandleDestroyed;
        }
    }

    private void HandleDestroyed()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (intactVisual != null)
        {
            intactVisual.SetActive(false);
        }

        if (destroyedVisual != null)
        {
            destroyedVisual.SetActive(true);
        }

        Destroy(gameObject, destroyDelay);
    }
}