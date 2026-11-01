
using UnityEngine;
using UnityEngine.UI;

public class ToxicitySystem : MonoBehaviour
{
    [Header("Toxicidad")]
    [Range(0, 100)]
    public float toxicidad = 30f;

    public Slider barraToxicidad;

    [Header("Tiempo de respuesta")]
    public float tiempoLimite = 10f;
    public float aumentoPorSegundo = 2f;

    private float tiempoSinResponder = 0f;
    private bool esperandoRespuesta = false;
    private bool llamadaActivada = false;

    public LlamadaLiam llamadaLiam;
    
   

    void Start()
    {
        ActualizarBarra();
    }

    void Update()
    {
        if (!esperandoRespuesta || llamadaActivada)
            return;

        tiempoSinResponder += Time.deltaTime;

        if (tiempoSinResponder > tiempoLimite)
        {
            CambiarToxicidad(aumentoPorSegundo * Time.deltaTime);
        }
    }

    public void RespuestaAsertiva()
    {
        Responder(-15f);
    }

    public void RespuestaSumisa()
    {
        Responder(-5f);
    }

    public void RespuestaAgresiva()
    {
        Responder(25f);
    }

    void Responder(float cantidad)
    {
        if (!esperandoRespuesta || llamadaActivada)
            return;

        esperandoRespuesta = false;
        CambiarToxicidad(cantidad);

        Debug.Log("Mensaje respondido.");
    }

    void CambiarToxicidad(float cantidad)
    {
        toxicidad = Mathf.Clamp(
            toxicidad + cantidad, 0f, 100f
        );

        ActualizarBarra();

        if (toxicidad >= 100f && !llamadaActivada)
        {
            llamadaActivada = true;

            Debug.Log("¡Toxicidad máxima!");

            if (llamadaLiam != null)
            llamadaLiam.IniciarLlamada();
        }
    }

    void ActualizarBarra()
    {
        if (barraToxicidad != null)
        {
            barraToxicidad.value = toxicidad;
        }
    }

    public void NuevoMensaje()
    {
        tiempoSinResponder = 0f;
        esperandoRespuesta = true;

        Debug.Log("Nuevo mensaje de Liam.");
    }
}


