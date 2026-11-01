
using UnityEngine;
using TMPro;
using System.Collections;

public class NotificacionCelular : MonoBehaviour
{
    public GameObject panelNotificacion;
    public TMP_Text textoNotificacion;

    public float duracion = 4f;

    private Coroutine notificacionActual;

    public void MostrarNotificacion(string mensaje)
    {
        if (notificacionActual != null)
            StopCoroutine(notificacionActual);

        textoNotificacion.text = mensaje;
        panelNotificacion.SetActive(true);

        notificacionActual =
            StartCoroutine(OcultarNotificacion());
    }

    IEnumerator OcultarNotificacion()
    {
        yield return new WaitForSecondsRealtime(duracion);

        panelNotificacion.SetActive(false);
        notificacionActual = null;
    }

    public void CerrarNotificacion()
    {
        if (notificacionActual != null)
        {
            StopCoroutine(notificacionActual);
            notificacionActual = null;
        }

        panelNotificacion.SetActive(false);
    }
}
