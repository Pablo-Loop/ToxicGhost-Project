using UnityEngine;

public class EndZoneInstructions : MonoBehaviour
{
    [Header("Referencias de Interfaz")]
    [Tooltip("Asigna aquí el Panel o Imagen del Canvas que contiene el mensaje/UI.")]
    [SerializeField] private GameObject panelCanvas;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entra es el Jugador (asegúrate de que el Jugador tenga la etiqueta "Player")
        if (other.CompareTag("Player"))
        {
            PausarYMostrarUI();
        }
    }

    private void PausarYMostrarUI()
    {
        // 1. Activar el panel/imagen del Canvas
        if (panelCanvas != null)
        {
            panelCanvas.SetActive(true);
        }

        // 2. Pausar el tiempo del juego
        Time.timeScale = 0f;

        // 3. Destruir el GameObject del Trigger para que no se vuelva a activar
        Destroy(gameObject);
    }
}
