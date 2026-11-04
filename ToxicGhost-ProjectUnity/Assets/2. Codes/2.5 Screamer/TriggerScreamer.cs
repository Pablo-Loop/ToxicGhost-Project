using UnityEngine;

public class TriggerScreamer : MonoBehaviour
{
    [Header("Configuración del Screamer")]
    [Tooltip("Objeto visual o animación del screamer (opcional). Si se asigna, se activará al entrar.")]
    [SerializeField] private GameObject objetoScreamer;

    [Tooltip("Si se marca, el trigger solo se ejecutará una vez en toda la partida.")]
    [SerializeField] private bool soloUnaVez = true;

    private bool yaActivado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (yaActivado && soloUnaVez) return;

        // Comprobar que sea el jugador (Gasp)
        if (other.CompareTag("Player"))
        {
            yaActivado = true;

            // 1. Reproducir el audio del Screamer desde el AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ReproducirScreamer();
            }
            else
            {
                Debug.LogWarning("[TriggerScreamer] No se encontró la instancia de AudioManager en la escena.");
            }

            // 2. Activar el elemento visual o animación si existe
            if (objetoScreamer != null)
            {
                objetoScreamer.SetActive(true);
            }

            // 3. Desactivar este trigger si es de un solo uso
            if (soloUnaVez)
            {
                // Si no tiene objeto visual adjunto, desactivamos el collider/gameobject directo
                if (objetoScreamer == null)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
