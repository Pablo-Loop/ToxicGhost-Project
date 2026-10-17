
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class ConversacionLiam
{
    [TextArea(2, 4)]
    public string mensajeLiam;

    [Header("Respuestas del jugador")]
    [TextArea(2, 4)]
    public string respuestaAsertiva;

    [TextArea(2, 4)]
    public string respuestaSumisa;

    [TextArea(2, 4)]
    public string respuestaAgresiva;

    [Header("Contestaciones de Liam")]
    [TextArea(2, 4)]
    public string liamAsertiva;

    [TextArea(2, 4)]
    public string liamSumisa;

    [TextArea(2, 4)]
    public string liamAgresiva;
}

public class ChatManager : MonoBehaviour
{
    [Header("Referencias")]
    public ToxicitySystem sistemaToxicidad;
    public CelularController celular;
    public TMP_Text textoMensaje;
    public NotificacionCelular notificacion;

    [Header("Botones")]
    public Button botonAsertiva;
    public Button botonSumisa;
    public Button botonAgresiva;

    [Header("Tiempo")]
    public float tiempoParaCerrar = 3f;

    [Header("Hilos de conversacion")]
    public ConversacionLiam[] conversaciones =
        new ConversacionLiam[5];

    private int mensajeActual = -1;
    private bool mensajeActivo = false;
    private bool esperandoCierre = false;

    void Start()
    {
        DesactivarBotones();
    }

    public bool RecibirMensaje(int numeroMensaje)
    {
        if (mensajeActivo || esperandoCierre)
            return false;

        if (numeroMensaje < 0 ||
            numeroMensaje >= conversaciones.Length)
            return false;

        if (conversaciones[numeroMensaje] == null)
            return false;

        mensajeActual = numeroMensaje;
        mensajeActivo = true;

        ConversacionLiam conversacion =
            conversaciones[mensajeActual];

        textoMensaje.text = conversacion.mensajeLiam;

        botonAsertiva.GetComponentInChildren<TMP_Text>().text =
            conversacion.respuestaAsertiva;

        botonSumisa.GetComponentInChildren<TMP_Text>().text =
            conversacion.respuestaSumisa;

        botonAgresiva.GetComponentInChildren<TMP_Text>().text =
            conversacion.respuestaAgresiva;

        ActivarBotones();
        sistemaToxicidad.NuevoMensaje();

        celular.CerrarCelular();

        notificacion.MostrarNotificacion(
            "Nuevo mensaje de Liam"
        );

        Debug.Log("Llego un mensaje de Liam.");

        return true;
    }

    public void ResponderAsertiva()
    {
        Responder(0);
    }

    public void ResponderSumisa()
    {
        Responder(1);
    }

    public void ResponderAgresiva()
    {
        Responder(2);
    }

    void Responder(int tipo)
    {
        if (!mensajeActivo || esperandoCierre)
            return;

        if (!botonAsertiva.interactable)
            return;

        notificacion.CerrarNotificacion();

        ConversacionLiam conversacion =
            conversaciones[mensajeActual];

        if (tipo == 0)
        {
            sistemaToxicidad.RespuestaAsertiva();
            textoMensaje.text = conversacion.liamAsertiva;
        }
        else if (tipo == 1)
        {
            sistemaToxicidad.RespuestaSumisa();
            textoMensaje.text = conversacion.liamSumisa;
        }
        else
        {
            sistemaToxicidad.RespuestaAgresiva();
            textoMensaje.text = conversacion.liamAgresiva;
        }

        DesactivarBotones();
        esperandoCierre = true;

        StartCoroutine(CerrarDespuesDeResponder());
    }

    IEnumerator CerrarDespuesDeResponder()
    {
        yield return new WaitForSecondsRealtime(
            tiempoParaCerrar
        );

        celular.CerrarCelular();

        mensajeActivo = false;
        esperandoCierre = false;
    }

    void ActivarBotones()
    {
        botonAsertiva.interactable = true;
        botonSumisa.interactable = true;
        botonAgresiva.interactable = true;
    }

    void DesactivarBotones()
    {
        botonAsertiva.interactable = false;
        botonSumisa.interactable = false;
        botonAgresiva.interactable = false;
    }
}

