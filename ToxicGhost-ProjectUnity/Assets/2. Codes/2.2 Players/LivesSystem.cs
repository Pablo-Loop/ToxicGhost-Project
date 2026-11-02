using UnityEngine;
using TMPro;

public class LivesSystem : MonoBehaviour
{
    [Header("Configuración de Vidas")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Interfaz de Usuario (UI)")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject gameOverCanvas;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);
        UpdateHealthUI();
    }


    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Vidas: " + currentHealth;
        }
    }

    private void Die()
    {
        isDead = true;

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
