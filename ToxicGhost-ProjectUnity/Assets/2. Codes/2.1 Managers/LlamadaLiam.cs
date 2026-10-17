
using UnityEngine;
using System.Collections;

public class LlamadaLiam : MonoBehaviour
{
    [Header("Configuracion")]
    public bool activarLlamada = true;

    [Min(1f)]
    public float duracionLlamada = 10f;

    private Coroutine rutinaDuracion;

    [Header("Interfaz")]
    public GameObject panelLlamada;

    [Header("Sonido")]
    public AudioSource audioLlamada;

    [Header("Vibracion Android")]
    public bool activarVibracion = true;

    [Min(0.5f)]
    public float intervaloVibracion = 1f;

    private bool llamando = false;
    private Coroutine rutinaVibracion;

    void Start()
    {
        if (panelLlamada != null)
            panelLlamada.SetActive(false);
    }

    public void IniciarLlamada()
    {
        if (!activarLlamada || llamando)
            return;

        llamando = true;

        if (panelLlamada != null)
            panelLlamada.SetActive(true);

        if (audioLlamada != null)
        {
            audioLlamada.loop = true;
            audioLlamada.Play();
        }

        
        if (activarVibracion)
        {
            rutinaVibracion = StartCoroutine(VibrarCelular());
        }

        rutinaDuracion = StartCoroutine(
            FinalizarLlamadaAutomaticamente()
        );

        Debug.Log("Liam esta llamando.");
    }

    IEnumerator VibrarCelular()
    {
        while (llamando)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.5f, intervaloVibracion)
            );
        }
    }



    IEnumerator FinalizarLlamadaAutomaticamente()
    {
        yield return new WaitForSecondsRealtime(
        Mathf.Max(1f, duracionLlamada)
        );

        rutinaDuracion = null;

        DetenerLlamada();

        Debug.Log("Liam dejo de llamar.");
    }

   
    public void DetenerLlamada()
    {
        llamando = false;

        if (rutinaDuracion != null)
        {
            StopCoroutine(rutinaDuracion);
            rutinaDuracion = null;
        }

        if (rutinaVibracion != null)
        {
            StopCoroutine(rutinaVibracion);
            rutinaVibracion = null;
        }

        if (audioLlamada != null)
            audioLlamada.Stop();

        if (panelLlamada != null)
            panelLlamada.SetActive(false);
    }


    void OnDisable()
    {
        DetenerLlamada();
    }
}
