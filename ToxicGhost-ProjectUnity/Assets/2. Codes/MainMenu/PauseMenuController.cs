
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private GameObject panelSettings;

    private bool estaPausado = false;

    private void Start()
    {
        Time.timeScale = 1f;
        estaPausado = false;

        panelPausa.SetActive(false);
        panelSettings.SetActive(false);
    }

    public void PausarJuego()
    {
        estaPausado = true;
        Time.timeScale = 0f;

        panelPausa.SetActive(true);
        panelSettings.SetActive(false);
    }

    public void ReanudarJuego()
    {
        estaPausado = false;
        Time.timeScale = 1f;

        panelPausa.SetActive(false);
        panelSettings.SetActive(false);
    }

    public void AbrirSettings()
    {
        panelPausa.SetActive(false);
        panelSettings.SetActive(true);
    }

    public void CerrarSettings()
    {
        panelSettings.SetActive(false);
        panelPausa.SetActive(true);
    }

    public void VolverMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

