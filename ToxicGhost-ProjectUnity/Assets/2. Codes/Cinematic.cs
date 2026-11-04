using UnityEngine;
using UnityEngine.Playables;

public class Cinematic : MonoBehaviour
{
    [Header("Configuración de Playable Director")]
    public PlayableDirector Scene;

    [Header("Objetos de Escena")]
    public GameObject SceneObj;    // Objeto contenedor de la cinemática (ej. "Escena Maya")
    public GameObject GameplayObj; // Objeto contenedor del Gameplay (Jugador, UI de control, etc.)

    [Header("Modo de Activación")]
    [Tooltip("Si se marca, la cinemática iniciará al entrar en el Collider Trigger de este objeto. Si no, arranca en Start().")]
    public bool activarPorTrigger = false;

    private float duracionCinematica;
    private float t;
    private bool cinematicaEnCurso = false;

    private void Start()
    {
        if (Scene != null)
        {
            duracionCinematica = (float)Scene.duration;
        }

        // Si NO es por trigger, inicia de inmediato al arrancar el juego
        if (!activarPorTrigger)
        {
            IniciarCinematica();
        }
        else
        {
            // Aseguramos que la escena de la cinemática empiece oculta si es por Trigger
            if (SceneObj != null) SceneObj.SetActive(false);
        }
    }

    private void Update()
    {
        // Solo contamos el tiempo si la cinemática ya inició
        if (!cinematicaEnCurso) return;

        t += Time.deltaTime;

        if (t >= duracionCinematica)
        {
            FinalizarCinematica();
        }
    }

    public void IniciarCinematica()
    {
        cinematicaEnCurso = true;
        t = 0f;

        // Activar objeto de cinemática y desactivar Gameplay
        if (SceneObj != null) SceneObj.SetActive(true);
        if (GameplayObj != null) GameplayObj.SetActive(false);

        // Reproducir el Timeline
        if (Scene != null)
        {
            Scene.time = 0;
            Scene.Play();
        }
    }

    private void FinalizarCinematica()
    {
        cinematicaEnCurso = false;

        // Desactivar la cinemática y reactivar el Gameplay
        if (SceneObj != null) SceneObj.SetActive(false);
        if (GameplayObj != null) GameplayObj.SetActive(true);

        // Destruimos el componente para que no vuelva a ejecutarse
        Destroy(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si la cinemática está configurada por trigger y el objeto que entra es Gasp/Jugador
        if (activarPorTrigger && !cinematicaEnCurso && other.CompareTag("Player"))
        {
            IniciarCinematica();
        }
    }
}