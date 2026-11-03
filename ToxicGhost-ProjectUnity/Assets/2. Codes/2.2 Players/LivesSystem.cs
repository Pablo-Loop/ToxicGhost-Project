
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LivesSystem : MonoBehaviour
{
    [Header("Configuración de Vidas")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Interfaz de Usuario (UI)")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject gameOverCanvas;

    [Header("Indicador visual de vidas")]
    [SerializeField] private Image imagenVidas;
    [SerializeField] private Sprite spriteTresVidas;
    [SerializeField] private Sprite spriteDosVidas;
    [SerializeField] private Sprite spriteUnaVida;

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
        // Mantener el texto original
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }

        // Actualizar imagen de Gasp
        if (imagenVidas != null)
        {
            if (currentHealth >= 3)
            {
                imagenVidas.sprite = spriteTresVidas;
            }
            else if (currentHealth == 2)
            {
                imagenVidas.sprite = spriteDosVidas;
            }
            else if (currentHealth == 1)
            {
                imagenVidas.sprite = spriteUnaVida;
            }
            else
            {
                imagenVidas.enabled = false;
            }
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

