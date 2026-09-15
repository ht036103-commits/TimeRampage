using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponConfig",
    menuName = "Time Rampage/Weapon Config"
)]
public sealed class WeaponConfig : ScriptableObject
{
    [Header("Base Weapon")]
    [Min(1)] public int damage = 10;
    [Min(0.1f)] public float shotsPerSecond = 6f;
    [Min(0.1f)] public float projectileSpeed = 28f;
    [Min(0.1f)] public float projectileLifetime = 2f;

    [Header("Scatter")]
    [Min(2)] public int scatterProjectileCount = 3;

    [Tooltip("Khoảng cách góc giữa hai viên đạn.")]
    [Range(1f, 45f)]
    public float scatterStepAngle = 12f;

}