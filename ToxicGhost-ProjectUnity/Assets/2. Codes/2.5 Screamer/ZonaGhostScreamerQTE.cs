
using UnityEngine;

public class ZonaGhostScreamerQTE : MonoBehaviour
{
    public GhostScreamerQTEController controlador;

    private void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player"))
            return;

        if (controlador != null)
        {
            controlador.IniciarEvento();
        }
    }
}
