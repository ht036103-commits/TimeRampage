using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyCountUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text enemyCountText;

    private EnemyActor[] enemies;
    private int remainingEnemies;

    private void Awake()
    {
        // Lấy các EnemyActor đang hoạt động trong Scene.
        enemies = FindObjectsOfType<EnemyActor>();

        remainingEnemies = 0;

        // Đếm số Enemy còn sống.
        foreach (EnemyActor enemy in enemies)
        {
            if (enemy != null &&
                enemy.Health != null &&
                !enemy.IsDead)
            {
                remainingEnemies++;
            }
        }
    }

    private void OnEnable()
    {
        if (enemies == null)
            return;

        // Đăng ký sự kiện khi Enemy bị tiêu diệt.
        foreach (EnemyActor enemy in enemies)
        {
            if (enemy != null && enemy.Health != null)
            {
                enemy.Health.Died += HandleEnemyDied;
            }
        }
    }

    private void Start()
    {
        UpdateEnemyCount();
    }

    private void OnDisable()
    {
        if (enemies == null)
            return;

        // Hủy đăng ký sự kiện khi UI bị tắt.
        foreach (EnemyActor enemy in enemies)
        {
            if (enemy != null && enemy.Health != null)
            {
                enemy.Health.Died -= HandleEnemyDied;
            }
        }
    }

    private void HandleEnemyDied()
    {
        remainingEnemies = Mathf.Max(
            0,
            remainingEnemies - 1
        );

        UpdateEnemyCount();
    }

    private void UpdateEnemyCount()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text =
                $"ENEMIES: {remainingEnemies}";
        }
    }
}
