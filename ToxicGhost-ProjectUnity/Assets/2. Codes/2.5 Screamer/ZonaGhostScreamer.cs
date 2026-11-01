
using UnityEngine;

public class ZonaGhostScreamer : MonoBehaviour
{
    [Header("Referencias")]
    public GhostScreamer fantasma;

    private bool activada = false;

    void OnTriggerEnter(Collider otro)
    {
        if (activada)
            return;

        if (!otro.CompareTag("Player"))
            return;

        if (fantasma == null)
            return;

        activada = true;

        fantasma.gameObject.SetActive(true);
        fantasma.Aparecer();

        Debug.Log("Ghost Screamer activado.");
    }
}
