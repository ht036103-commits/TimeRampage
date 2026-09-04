using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerMovementConfig",
    menuName = "Time Rampage/Player Movement Config"
)]
public sealed class PlayerMovementConfig : ScriptableObject
{
    [Header("Horizontal Movement")]
    [Min(0f)] public float maxRunSpeed = 8f;
    [Min(0f)] public float groundAcceleration = 75f;
    [Min(0f)] public float groundDeceleration = 90f;
    [Min(0f)] public float airAcceleration = 45f;
    [Min(0f)] public float airDeceleration = 20f;

    [Header("Jump")]
    [Min(0f)] public float jumpHeight = 2.6f;
    [Min(0f)] public float doubleJumpHeight = 2.3f;
    [Min(0f)] public int extraAirJumps = 1;

    [Tooltip("Trọng lực khi nhân vật đang bay lên.")]
    [Min(0f)] public float upwardGravity = 42f;

    [Tooltip("Trọng lực khi nhân vật đang rơi xuống.")]
    [Min(0f)] public float fallingGravity = 60f;

    [Tooltip("Làm nhân vật rơi sớm khi người chơi thả nút Jump.")]
    [Min(1f)] public float jumpCutGravityMultiplier = 2.3f;

    [Tooltip("Ngưỡng vận tốc để xác định nhân vật gần đỉnh cú nhảy.")]
    [Min(0f)] public float apexVelocityThreshold = 1.2f;

    [Tooltip("Giảm trọng lực gần đỉnh cú nhảy.")]
    [Range(0.1f, 1f)] public float apexGravityMultiplier = 0.55f;

    [Min(0f)] public float maximumFallSpeed = 25f;

    [Header("Forgiveness")]
    [Tooltip("Cho phép nhảy trong một khoảng ngắn sau khi rời platform.")]
    [Range(0f, 0.5f)] public float coyoteTime = 0.14f;

    [Tooltip("Ghi nhớ nút Jump được bấm trước khi chạm đất.")]
    [Range(0f, 0.5f)] public float jumpBufferTime = 0.14f;

    [Header("Dash")]
    [Min(0f)] public float dashSpeed = 17f;
    [Min(0f)] public float dashDuration = 0.16f;
    [Min(0f)] public float dashCooldown = 0.35f;
    [Min(0)] public int airDashes = 1;
}