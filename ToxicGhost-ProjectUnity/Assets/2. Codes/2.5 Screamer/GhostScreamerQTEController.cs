
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GhostScreamerQTEController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject prefabFantasma;
    public Transform puntoSpawn;
    public Transform jugador;
    public Transform camaraJugador;

    [Header("Interfaz QTE")]
    public GameObject textoReacciona;
    public Button botonElectrocutar;
    public RectTransform areaCanvas;

    [Header("Controles del jugador")]
    public MonoBehaviour controlMovimiento;
    public CameraLookArea controlCamara;

    private bool movimientoEstabaActivo;
    private bool camaraEstabaActiva;

    [Header("Configuracion")]
    public float velocidadFantasma = 1.5f;
    public float distanciaScreamer = 1.5f;
    public float distanciaAparicion = 4f;

    private GameObject fantasmaActual;
    private FantasmaQTE movimientoFantasma;
    private bool eventoActivo = false;
  

    [Header("Tiempo y Screamer")]
    public float tiempoLimite = 6f;
    public GameObject imagenScreamer;
    public AudioSource sonidoScreamer;
    public float duracionScreamer = 2f;

    private float tiempoRestante;
    private bool mostrandoScreamer = false;

    public bool IniciarEvento(Transform spawnDeZona)
    {
        // No iniciar otro QTE mientras haya uno activo
        // o mientras se esté mostrando el Screamer.
        if (eventoActivo || mostrandoScreamer)
            return false;

        if (prefabFantasma == null ||
            spawnDeZona == null ||
            jugador == null ||
            camaraJugador == null)
        {
            Debug.LogWarning("Faltan referencias del Ghost Screamer QTE.");
            return false;
        }

        // Comprobar el prefab antes de bloquear controles.
        if (prefabFantasma.GetComponent<FantasmaQTE>() == null)
        {
            Debug.LogError("El prefab no tiene el script FantasmaQTE.");
            return false;
        }

        // Obtener la dirección horizontal hacia donde mira el jugador
        Vector3 direccionFrente = camaraJugador.forward;
        direccionFrente.y = 0f;
        direccionFrente.Normalize();

        // Calcular posición frente al jugador
        Vector3 posicionAparicion =
            jugador.position + direccionFrente * distanciaAparicion;

        // Mantener la altura definida por el punto de spawn de la zona
        posicionAparicion.y = spawnDeZona.position.y;

        // Hacer que el fantasma mire hacia el jugador
        Quaternion rotacionAparicion =
            Quaternion.LookRotation(-direccionFrente, Vector3.up);

        // Crear fantasma
        fantasmaActual = Instantiate(
            prefabFantasma,
            posicionAparicion,
            rotacionAparicion
        );

        movimientoFantasma = fantasmaActual.GetComponent<FantasmaQTE>();

        eventoActivo = true;
        tiempoRestante = tiempoLimite;

        BloquearControles();

        movimientoFantasma.velocidad = velocidadFantasma;
        movimientoFantasma.distanciaScreamer = distanciaScreamer;
        movimientoFantasma.IniciarPersecucion(jugador);

        if (textoReacciona != null)
            textoReacciona.SetActive(true);

        if (botonElectrocutar != null)
        {
            botonElectrocutar.gameObject.SetActive(true);
            ColocarBotonAleatorio();
        }

        Debug.Log("Ghost Screamer QTE iniciado.");
        return true;
    }

     
    void Update()
    {
        if (!eventoActivo || movimientoFantasma == null)
            return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f ||
            movimientoFantasma.EstaCercaDelJugador())
        {
            Debug.Log("¡Gasp no reaccionó a tiempo!");
            ActivarScreamer();
        }
    }

    void FinalizarEvento()
    {
        eventoActivo = false;
        RestaurarControles();

        if (fantasmaActual != null)
            Destroy(fantasmaActual);

        Debug.Log("Evento Ghost Screamer finalizado.");
        if (textoReacciona != null)
            textoReacciona.SetActive(false);

        if (botonElectrocutar != null)
            botonElectrocutar.gameObject.SetActive(false);
    }

    void ColocarBotonAleatorio()
    {
        if (botonElectrocutar == null || areaCanvas == null)
            return;

        RectTransform botonRect = botonElectrocutar.GetComponent<RectTransform>();

        // Trabajamos con el centro del Canvas
        botonRect.anchorMin = new Vector2(0.5f, 0.5f);
        botonRect.anchorMax = new Vector2(0.5f, 0.5f);
        botonRect.pivot = new Vector2(0.5f, 0.5f);

        float limiteX = Mathf.Max(0f,
            (areaCanvas.rect.width - botonRect.rect.width) / 2f);

        float limiteY = Mathf.Max(0f,
            (areaCanvas.rect.height - botonRect.rect.height) / 2f);

        botonRect.anchoredPosition = new Vector2(
            Random.Range(-limiteX, limiteX),
            Random.Range(-limiteY, limiteY)
        );
    }

        public void ElectrocutarFantasma()
    {
        if (!eventoActivo)
            return;

        Debug.Log("¡Gasp electrocutó al fantasma!");

        FinalizarEvento();
    }

        void BloquearControles()
    {
        if (controlMovimiento != null)
        {
            movimientoEstabaActivo = controlMovimiento.enabled;
            controlMovimiento.enabled = false;
        }

        if (controlCamara != null)
        {
            camaraEstabaActiva = controlCamara.enabled;
            controlCamara.enabled = false;
        }

        Debug.Log("Controles bloqueados.");
    }

        void RestaurarControles()
    {
        if (controlMovimiento != null)
            controlMovimiento.enabled = movimientoEstabaActivo;

        if (controlCamara != null)
            controlCamara.enabled = camaraEstabaActiva;

        Debug.Log("Controles restaurados.");
    }

        void ActivarScreamer()
    {
        if (mostrandoScreamer)
            return;

        mostrandoScreamer = true;
        eventoActivo = false;

        if (fantasmaActual != null)
            Destroy(fantasmaActual);

        if (textoReacciona != null)
            textoReacciona.SetActive(false);

        if (botonElectrocutar != null)
            botonElectrocutar.gameObject.SetActive(false);

        StartCoroutine(MostrarScreamer());
    }

    IEnumerator MostrarScreamer()
    {
        if (imagenScreamer != null)
            imagenScreamer.SetActive(true);

        if (sonidoScreamer != null)
            sonidoScreamer.Play();

        yield return new WaitForSeconds(duracionScreamer);

        if (imagenScreamer != null)
            imagenScreamer.SetActive(false);

        mostrandoScreamer = false;

        RestaurarControles();

        Debug.Log("Screamer finalizado. Controles restaurados.");
    }
}
