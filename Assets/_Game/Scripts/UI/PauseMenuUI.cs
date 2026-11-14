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

    private void Start()
    {
        // Dam bao game bat dau voi thoi gian binh thuong
        Time.timeScale = 1f;

        // An Pause Menu khi bat dau man choi
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Pause Menu Panel chua duoc gan!");
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

        pauseMenuPanel.SetActive(true);

        // Tam dung thoi gian trong game
        Time.timeScale = 0f;

        // Khong cho nhan nut Pause them lan nua
        if (pauseButton != null)
        {
            pauseButton.interactable = false;
        }
    }

    // Dong Pause Menu va tiep tuc game
    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Khoi phuc thoi gian game
        Time.timeScale = 1f;

        if (pauseButton != null)
        {
            pauseButton.interactable = true;
        }
    }

    // Khoi dong lai man choi hien tai
    public void RestartGame()
    {
        // Phai khoi phuc thoi gian truoc khi tai lai scene
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
