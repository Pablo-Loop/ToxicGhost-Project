
using UnityEngine;
using System.Collections;

public class GhostScreamer : MonoBehaviour
{
    [Header("Configuracion")]
    public float duracionAparicion = 2f;

    [Header("Sonido")]
    public AudioSource audioSusto;

    private Camera camaraPrincipal;
    private bool apareciendo = false;

    void Awake()
    {
        camaraPrincipal = Camera.main;
    }

    void LateUpdate()
    {
        if (camaraPrincipal == null)
            return;

        // Orientar el sprite hacia la camara.
        transform.rotation = camaraPrincipal.transform.rotation;
    }

    public void Aparecer()
    {
        if (apareciendo)
            return;

        gameObject.SetActive(true);
        StartCoroutine(MostrarFantasma());
    }

    IEnumerator MostrarFantasma()
    {
        apareciendo = true;

        if (audioSusto != null)
        {
            audioSusto.Play();
        }

        yield return new WaitForSeconds(duracionAparicion);

        apareciendo = false;
        gameObject.SetActive(false);
    }
}

