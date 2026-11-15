using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;

    private bool gameOverShown;

    private void Awake()
    {
        // An Game Over Panel khi bat dau
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (playerHealth == null)
        {
            Debug.LogError(
                "GameOverUI: Chua gan Player Health!",
                this
            );
        }

        if (gameOverPanel == null)
        {
            Debug.LogError(
                "GameOverUI: Chua gan Game Over Panel!",
                this
            );
        }
    }

    private void OnEnable()
    {
        // Lang nghe su kien Player chet
        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDied;
        }
    }

    private void OnDisable()
    {
        // Huy dang ky su kien
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }
    }

    private void HandlePlayerDied()
    {
        if (gameOverShown)
        {
            return;
        }

        gameOverShown = true;

        // An Pause Menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // An Settings Menu
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }

        // Khong cho mo Pause Menu sau khi Game Over
        if (pauseButton != null)
        {
            pauseButton.interactable = false;
        }

        // Hien thi Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Dung toan bo thoi gian game
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        // Khoi phuc thoi gian truoc khi tai lai Scene
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

