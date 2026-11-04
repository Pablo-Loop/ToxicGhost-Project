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

    [Header("Opciones Post-Cinemática")]
    [Tooltip("MARCAR en la cinemática inicial para abrir Hoja1 al terminar.")]
    public bool mostrarUIPostCinematica = true;

    private bool cinematicaEnCurso = false;

    private void OnEnable()
    {
        if (Scene != null)
        {
            // Escuchar el evento oficial de finalización del Timeline
            Scene.stopped += OnTimelineStopped;
        }
    }

    private void OnDisable()
    {
        if (Scene != null)
        {
            Scene.stopped -= OnTimelineStopped;
        }
    }

    private void Start()
    {
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
        Time.timeScale = 1f;

        // 1. Activar cinemática y DESACTIVAR Gameplay para que no estorbe el HUD
        if (SceneObj != null) SceneObj.SetActive(true);
        if (GameplayObj != null) GameplayObj.SetActive(false);

        // 2. Reproducir Timeline
        if (Scene != null)
        {
            Scene.time = 0;
            Scene.Evaluate();
            Scene.Play();
        }
    }

    // Se dispara automáticamente cuando la barra de tiempo del PlayableDirector llega al final exacto
    private void OnTimelineStopped(PlayableDirector director)
    {
        if (director == Scene && cinematicaEnCurso)
        {
            FinalizarCinematica();
        }
    }

    private void FinalizarCinematica()
    {
        cinematicaEnCurso = false;

        // 1. Apagar la cinemática y activar ahora sí el Gameplay
        if (SceneObj != null) SceneObj.SetActive(false);
        if (GameplayObj != null) GameplayObj.SetActive(true);

       
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