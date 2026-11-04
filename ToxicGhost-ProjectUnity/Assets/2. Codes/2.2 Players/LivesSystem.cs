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

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

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

        // Reproducir sonido de daño
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirPerderVida();
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool Heal(int amount)
    {
        // Si ya está muerto o la vida está al máximo, no consume el pickup
        if (isDead || currentHealth >= maxHealth) return false;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Reproducir sonido de recuperación de vida
        if (AudioManager.Instance != null && AudioManager.Instance.sfxPickUpVida != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxPickUpVida);
        }

        UpdateHealthUI();
        return true;
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }

        if (imagenVidas != null)
        {
            imagenVidas.enabled = true;

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