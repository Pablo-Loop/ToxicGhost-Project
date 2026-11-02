
using UnityEngine;

public class ZonaGhostScreamerQTE : MonoBehaviour
{
    [Header("Referencias")]
    public GhostScreamerQTEController controlador;
    public Transform puntoSpawn;

    [Header("Configuracion")]
    public bool activarUnaSolaVez = true;

    private bool zonaUtilizada = false;

    private void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player"))
            return;

        if (activarUnaSolaVez && zonaUtilizada)
            return;

        if (controlador == null || puntoSpawn == null)
        {
            Debug.LogWarning(
                "Faltan referencias en la zona Ghost Screamer QTE."
            );
            return;
        }

        // Solo registrar la zona como utilizada
        // si el GameManager inició el evento.
        bool eventoIniciado = controlador.IniciarEvento(puntoSpawn);

        if (eventoIniciado && activarUnaSolaVez)
        {
            zonaUtilizada = true;
        }
    }
}

