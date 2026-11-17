using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class EnemyCountUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text enemyCountText;

    private EnemyActor[] enemies;
    private int remainingEnemies;
    private bool allEnemiesDefeatedNotified;

    public int RemainingEnemies => remainingEnemies;

    public bool AreAllEnemiesDefeated =>
        remainingEnemies <= 0;

    public event Action AllEnemiesDefeated;

    private void Awake()
    {
        enemies = FindObjectsOfType<EnemyActor>();

        remainingEnemies = 0;

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
        NotifyIfAllEnemiesDefeated();
    }

    private void OnDisable()
    {
        if (enemies == null)
            return;

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
        NotifyIfAllEnemiesDefeated();
    }

    private void NotifyIfAllEnemiesDefeated()
    {
        if (remainingEnemies > 0 ||
            allEnemiesDefeatedNotified)
        {
            return;
        }

        allEnemiesDefeatedNotified = true;

        AllEnemiesDefeated?.Invoke();
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