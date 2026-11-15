using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Menu References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button pauseButton;

    [Header("Settings Menu References")]
    [SerializeField] private GameObject settingsMenuPanel;

    private void Start()
    {
        // Dam bao game bat dau voi thoi gian binh thuong
        Time.timeScale = 1f;

        // An cac menu khi bat dau man choi
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Pause Menu Panel chua duoc gan!");
        }

        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Settings Menu Panel chua duoc gan!");
        }
    }

    // Mo Pause Menu va tam dung game
    public void PauseGame()
    {
        if (pauseMenuPanel == null)
        {
            Debug.LogError("Pause Menu Panel chua duoc gan!");
            return;
        }

        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }

        pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;

        if (pauseButton != null)
        {
            pauseButton.interactable = false;
        }
    }

    // Mo Settings Menu tu Pause Menu
    public void OpenSettings()
    {
        if (pauseMenuPanel == null || settingsMenuPanel == null)
        {
            Debug.LogError("Pause Menu hoac Settings Menu chua duoc gan!");
            return;
        }

        pauseMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);

        // Khong khoi phuc Time.timeScale.
        // Game tiep tuc o trang thai tam dung.
    }

    // Quay tu Settings Menu ve Pause Menu
    public void BackToPauseMenu()
    {
        if (pauseMenuPanel == null || settingsMenuPanel == null)
        {
            Debug.LogError("Pause Menu hoac Settings Menu chua duoc gan!");
            return;
        }

        settingsMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);

        // Dam bao game van dang tam dung
        Time.timeScale = 0f;
    }

    // Dong menu va tiep tuc game
    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        if (pauseButton != null)
        {
            pauseButton.interactable = true;
        }
    }

    // Khoi dong lai man choi hien tai
    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void OnDestroy()
    {
        // Dam bao game khong bi giu o trang thai tam dung
        Time.timeScale = 1f;
    }
}
