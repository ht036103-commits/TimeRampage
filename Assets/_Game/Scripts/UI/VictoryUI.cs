using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryUI : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private EnemyCountUI enemyCountUI;
    [SerializeField] private Health playerHealth;

    [Header("UI Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;

    private bool playerReachedExit;
    private bool victoryShown;

    private void Awake()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (enemyCountUI == null)
        {
            Debug.LogError(
                "VictoryUI: Chua gan EnemyCountUI!",
                this
            );
        }

        if (playerHealth == null)
        {
            Debug.LogError(
                "VictoryUI: Chua gan Player Health!",
                this
            );
        }

        if (victoryPanel == null)
        {
            Debug.LogError(
                "VictoryUI: Chua gan Victory Panel!",
                this
            );
        }
    }

    private void OnEnable()
    {
        if (enemyCountUI != null)
        {
            enemyCountUI.AllEnemiesDefeated +=
                HandleAllEnemiesDefeated;
        }
    }

    private void Start()
    {
        // Xu ly truong hop tat ca enemy da chet
        // truoc khi VictoryUI bat dau.
        if (enemyCountUI != null &&
            enemyCountUI.AreAllEnemiesDefeated)
        {
            HandleAllEnemiesDefeated();
        }
    }

    private void OnDisable()
    {
        if (enemyCountUI != null)
        {
            enemyCountUI.AllEnemiesDefeated -=
                HandleAllEnemiesDefeated;
        }
    }

    public void PlayerReachedExit()
    {
        if (victoryShown)
            return;

        playerReachedExit = true;

        TryShowVictory();
    }

    private void HandleAllEnemiesDefeated()
    {
        TryShowVictory();
    }

    private void TryShowVictory()
    {
        if (victoryShown ||
            !playerReachedExit ||
            enemyCountUI == null ||
            !enemyCountUI.AreAllEnemiesDefeated)
        {
            return;
        }

        if (playerHealth == null ||
            playerHealth.IsDead)
        {
            return;
        }

        victoryShown = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }

        if (pauseButton != null)
        {
            pauseButton.interactable = false;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Application.Quit();
    }
}
