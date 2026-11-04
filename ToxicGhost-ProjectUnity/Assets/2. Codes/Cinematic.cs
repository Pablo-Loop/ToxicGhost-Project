using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class Cinematic : MonoBehaviour
{
    [Header("Configuración de Playable Director")]
    public PlayableDirector Scene;

    [Header("Objetos de Escena")]
    public GameObject SceneObj;    // Objeto contenedor de la cinemática ("Escena")
    public GameObject GameplayObj; // Objeto contenedor del Gameplay ("Gameplay")

    [Header("Modo de Activación")]
    [Tooltip("DESMARCAR en la cinemática inicial para que arranque sola al iniciar el juego.")]
    public bool activarPorTrigger = false;

    [Header("Opciones Post-Cinemática (Opcional)")]
    [Tooltip("MARCAR en la cinemática inicial para abrir Hoja1 al terminar y pausar el juego.")]
    public bool mostrarUIPostCinematica = false;

    private float duracionCinematica;
    private bool cinematicaEnCurso = false;

    private void Start()
    {
        if (Scene != null)
        {
            duracionCinematica = (float)Scene.duration;
        }

        if (!activarPorTrigger)
        {
            IniciarCinematica();
        }
        else
        {
            if (SceneObj != null) SceneObj.SetActive(false);
        }
    }

    public void IniciarCinematica()
    {
        if (cinematicaEnCurso) return;

        cinematicaEnCurso = true;
        Time.timeScale = 1f; // Garantizar velocidad normal durante la cinemática

        // 1. Activar contenedor de la cinemática y asegurar Gameplay activo para mantener cámaras
        if (SceneObj != null) SceneObj.SetActive(true);
        if (GameplayObj != null) GameplayObj.SetActive(true);

        // 2. Reproducir Timeline
        if (Scene != null)
        {
            Scene.time = 0;
            Scene.Evaluate();
            Scene.Play();
        }

        // Respaldo de tiempo por si la duración reportada es 0
        float tiempoEspera = (duracionCinematica > 0f) ? duracionCinematica : 3f;
        StartCoroutine(EsperaFinCinematica(tiempoEspera));
    }

    private IEnumerator EsperaFinCinematica(float duracion)
    {
        yield return new WaitForSecondsRealtime(duracion);
        FinalizarCinematica();
    }

    private void FinalizarCinematica()
    {
        cinematicaEnCurso = false;

        // 1. Apagar únicamente el objeto visual de la cinemática
        if (SceneObj != null) SceneObj.SetActive(false);
        if (GameplayObj != null) GameplayObj.SetActive(true);

        // 2. Notificar al UImanager para abrir la Hoja 1 y pausar el juego
        if (mostrarUIPostCinematica)
        {
            if (UImanager.Instance != null)
            {
                UImanager.Instance.MostrarHojaUIExclusiva();
            }
            else
            {
                UImanager manager = FindFirstObjectByType<UImanager>();
                if (manager != null)
                {
                    manager.MostrarHojaUIExclusiva();
                }
                else
                {
                    Debug.LogError("[Cinematic] No se encontró el componente UImanager en la escena.");
                }
            }
        }

        // 3. Destruir el componente del script al concluir
        Destroy(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activarPorTrigger && !cinematicaEnCurso && other.CompareTag("Player"))
        {
            IniciarCinematica();
        }
    }
}