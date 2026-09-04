using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ScatterPickup : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private float rotationSpeed = 90f;

    private void Update()
    {
        transform.Rotate(
            0f,
            rotationSpeed * Time.deltaTime,
            0f,
            Space.World
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        WeaponController weapon =
            other.GetComponentInParent<
                WeaponController
            >();

        if (weapon == null)
        {
            return;
        }

        weapon.ActivateScatter(duration);

        Destroy(gameObject);
    }
} 