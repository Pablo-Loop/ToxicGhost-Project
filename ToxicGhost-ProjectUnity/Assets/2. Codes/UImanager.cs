using UnityEngine;

public class UImanager : MonoBehaviour
{
    public static UImanager Instance { get; private set; }

    [Header("Referencias de Interfaz")]
    [Tooltip("La hoja informativa que se muestra al terminar la cinemática (contiene la 'X').")]
    [SerializeField] private GameObject panelHojaUI;

    [Tooltip("Objeto padre que contiene todas las demás UIs del juego (Joystick, vida, etc.).")]
    [SerializeField] private GameObject contenedorHUDGameplay;

    private void Start()
    {
        // 1. Asegurar tiempo normal al iniciar la escena para que la cinemática se reproduzca sin trabarse
        Time.timeScale = 1f;

        // 2. Ocultar la Hoja1 y el HUD mientras transcurre la cinemática inicial
        if (panelHojaUI != null) panelHojaUI.SetActive(false);
        if (contenedorHUDGameplay != null) contenedorHUDGameplay.SetActive(false);
    }

    /// <summary>
    /// Se llama automáticamente desde Cinematic.cs solo al finalizar la cinemática inicial.
    /// </summary>
    public void MostrarHojaUIExclusiva()
    {
        // Congelar físicas y movimientos del juego al terminar la secuencia
        Time.timeScale = 0f;

        // Mostrar exclusivamente la Hoja1 (con la X)
        if (panelHojaUI != null) panelHojaUI.SetActive(true);
        if (contenedorHUDGameplay != null) contenedorHUDGameplay.SetActive(false);
    }

    /// <summary>
    /// Vincula esta función al evento On Click() del botón 'X' de Hoja1.
    /// </summary>
    public void ReanudarJuegoYActivarHUD()
    {
        if (panelHojaUI != null) panelHojaUI.SetActive(false);

        if (contenedorHUDGameplay != null) contenedorHUDGameplay.SetActive(true);

        Time.timeScale = 1f;
    }
}