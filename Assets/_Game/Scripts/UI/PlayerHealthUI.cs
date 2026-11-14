using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TMP_Text healthText;

    private void Awake()
    {
        if (playerHealth == null)
        {
            Debug.LogError(
                "PlayerHealthUI: Missing Health reference.",
                this
            );
        }
    }

    private void OnEnable()
    {
        if (playerHealth == null)
            return;

        playerHealth.HealthChanged += UpdateHealthUI;
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            UpdateHealthUI(
                playerHealth.CurrentHealth,
                playerHealth.MaximumHealth
            );
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= UpdateHealthUI;
        }
    }

    private void UpdateHealthUI(
        int currentHealth,
        int maximumHealth
    )
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount =
                (float)currentHealth / maximumHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{currentHealth} / {maximumHealth} HP";
        }
    }
}
