
using UnityEngine;

public class ZonaMensaje : MonoBehaviour
{
    public ChatManager chatManager;

    [Header("Mensaje que activa esta zona")]
    public int numeroMensaje = 0;

    private bool zonaActivada = false;

    private void OnTriggerEnter(Collider otro)
    {
        if (zonaActivada) return;

        if (!otro.CompareTag("Player")) return;

        if (chatManager == null) return;

        bool mensajeEnviado =
            chatManager.RecibirMensaje(numeroMensaje);

        if (mensajeEnviado)
        {
            zonaActivada = true;

            Debug.Log("Zona de mensaje activada.");
        }
    }
}
