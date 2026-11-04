using System.Collections.Generic;
using UnityEngine;

public class UImanager : MonoBehaviour
{
    public static UImanager Instance { get; private set; }

    [Header("Colección de Hojas Informativas")]
    [Tooltip("Agrega las Hojas UI en orden: Element 0 = Hoja1, Element 1 = Hoja2, Element 2 = Hoja3, etc.")]
    [SerializeField] private List<GameObject> listaHojasUI = new List<GameObject>();

    [Header("HUD del Juego")]
    [Tooltip("Objeto padre que contiene las UIs del gameplay (Joystick, vida, celular, etc.).")]
    [SerializeField] private GameObject contenedorHUDGameplay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        OcultarTodasLasHojas();

        if (contenedorHUDGameplay != null)
        {
            contenedorHUDGameplay.SetActive(false);
        }
    }

    private void OcultarTodasLasHojas()
    {
        foreach (GameObject hoja in listaHojasUI)
        {
            if (hoja != null) hoja.SetActive(false);
        }
    }

    #region Hoja 1 (Post-Cinemática Inicial)

    /// <summary>
    /// Se llama automáticamente desde Cinematic.cs al finalizar la animación.
    /// </summary>
    public void MostrarHojaUIExclusiva()
    {
        MostrarHojaPorIndice(0); // Element 0 corresponde a Hoja1
    }

    #endregion

    #region Control Genérico de Hojas (Triggers)

    /// <summary>
    /// Abre cualquier hoja según su índice en la lista y congela el tiempo del juego.
    /// </summary>
    public void MostrarHojaPorIndice(int indiceHoja)
    {
        if (indiceHoja < 0 || indiceHoja >= listaHojasUI.Count)
        {
            Debug.LogWarning($"[UImanager] El índice {indiceHoja} no existe en listaHojasUI.");
            return;
        }

        OcultarTodasLasHojas();

        if (listaHojasUI[indiceHoja] != null)
        {
            listaHojasUI[indiceHoja].SetActive(true);
            listaHojasUI[indiceHoja].transform.SetAsLastSibling();
        }

        if (contenedorHUDGameplay != null)
        {
            contenedorHUDGameplay.SetActive(false);
        }

        Time.timeScale = 0f; // Congela el juego
    }

    /// <summary>
    /// Asigna esta función al evento On Click() del botón 'X' de TODAS las Hojas.
    /// </summary>
    public void CerrarHojaActualYReanudar()
    {
        OcultarTodasLasHojas();

        if (contenedorHUDGameplay != null)
        {
            contenedorHUDGameplay.SetActive(true);
        }

        Time.timeScale = 1f; // Reanuda el juego
    }

    #endregion
}