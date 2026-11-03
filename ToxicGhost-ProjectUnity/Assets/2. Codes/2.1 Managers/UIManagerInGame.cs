using System.Collections;
using UnityEngine;
using TMPro; // Necesario para trabajar con TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("Configuración del Mensaje")]
    [Tooltip("Asigna aquí el componente TMP_Text de tu interfaz.")]
    [SerializeField] private TMP_Text textoMensaje;

    [Tooltip("Tiempo en segundos que el texto permanecerá visible.")]
    [SerializeField] private float duracionVisibilidad = 10f;

    void Start()
    {
        if (textoMensaje != null)
        {
            StartCoroutine(MostrarYOcultarTexto());
        }
        else
        {
            Debug.LogWarning("No has asignado el TMP_Text en el Inspector de UIManager.");
        }
    }

    private IEnumerator MostrarYOcultarTexto()
    {
        // 1. Asegurar que el texto esté visible al iniciar
        textoMensaje.gameObject.SetActive(true);

        // 2. Esperar el tiempo configurado
        yield return new WaitForSeconds(duracionVisibilidad);

        // 3. Ocultar el objeto del texto tras transcurrir el tiempo
        textoMensaje.gameObject.SetActive(false);
    }
}