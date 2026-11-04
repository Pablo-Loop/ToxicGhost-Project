
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panel de configuración")]
    [SerializeField] private GameObject panelSettings;

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainWorld");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void OpenSettings()
    {
        panelSettings.SetActive(true);
    }

    public void CloseSettings()
    {
        panelSettings.SetActive(false);
    }
}
