using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager Instance { get; private set; }

    [Header("Referencias Principales")]
    [Tooltip("Asigna únicamente el Transform del Jugador (Gasp).")]
    [SerializeField] private Transform jugadorTransform;

    // Listas internas que se llenarán automáticamente por Tags
    private List<NavMeshAgent> fantasmasEnEscena = new List<NavMeshAgent>();
    private List<GameObject> objetosGuardables = new List<GameObject>();

    private GameData datosActuales = new GameData();
    private const string SAVE_KEY = "DontGhostMe_SaveData";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InicializarReferenciasPorTags();
        CargarEstadoJuego();
    }

    /// <summary>
    /// Escanea la escena y registra automáticamente todos los objetos marcados con Tags.
    /// </summary>
    private void InicializarReferenciasPorTags()
    {
        // 1. Buscar automáticamente todos los objetos con Tag "Saveable"
        GameObject[] encontrados = GameObject.FindGameObjectsWithTag("Saveable");
        objetosGuardables.Clear();
        objetosGuardables.AddRange(encontrados);

        // 2. Buscar automáticamente todos los fantasmas con Tag "Ghost"
        GameObject[] fantasmasObj = GameObject.FindGameObjectsWithTag("Player");
        fantasmasEnEscena.Clear();

        foreach (GameObject g in fantasmasObj)
        {
            NavMeshAgent agent = g.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                fantasmasEnEscena.Add(agent);
            }
        }

        Debug.Log($"Referencias inicializadas: {objetosGuardables.Count} objetos guardables y {fantasmasEnEscena.Count} fantasmas detectados.");
    }

    #region Guardado y Cargado

    public void GuardarEstadoJuego()
    {
        if (jugadorTransform == null) return;

        // 1. Guardar Jugador
        datosActuales.posicionJugador = new PosicionData(jugadorTransform.position);

        // 2. Guardar Fantasmas (NavMesh)
        datosActuales.listaFantasmas.Clear();
        for (int i = 0; i < fantasmasEnEscena.Count; i++)
        {
            if (fantasmasEnEscena[i] != null)
            {
                FantasmaData fData = new FantasmaData
                {
                    idFantasma = fantasmasEnEscena[i].gameObject.name + "_" + i,
                    posicion = new PosicionData(fantasmasEnEscena[i].transform.position),
                    estaActivo = fantasmasEnEscena[i].gameObject.activeSelf
                };
                datosActuales.listaFantasmas.Add(fData);
            }
        }

        // 3. Guardar Objetos del Escenario (Triggers, Pistas, Coleccionables)
        datosActuales.listaObjetosEscenario.Clear();
        for (int i = 0; i < objetosGuardables.Count; i++)
        {
            if (objetosGuardables[i] != null)
            {
                ObjetoEscenarioData oData = new ObjetoEscenarioData
                {
                    idObjeto = objetosGuardables[i].name,
                    estaRecolectadoODesactivado = !objetosGuardables[i].activeSelf
                };
                datosActuales.listaObjetosEscenario.Add(oData);
            }
        }

        // 4. Convertir a JSON y guardar en PlayerPrefs
        string json = JsonUtility.ToJson(datosActuales);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("Juego guardado automáticamente mediante Tags.");
    }

    public void CargarEstadoJuego()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        datosActuales = JsonUtility.FromJson<GameData>(json);

        // 1. Restaurar Posición del Jugador
        if (jugadorTransform != null && datosActuales.posicionJugador != null)
        {
            Vector3 targetPos = datosActuales.posicionJugador.ToVector3();
            targetPos.y += 0.1f; // Holgura para evitar colisiones con el suelo

            CharacterController cc = jugadorTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            jugadorTransform.position = targetPos;

            if (cc != null) cc.enabled = true;
        }

        // 2. Restaurar Fantasmas usando NavMeshAgent.Warp()
        for (int i = 0; i < fantasmasEnEscena.Count; i++)
        {
            if (fantasmasEnEscena[i] != null && i < datosActuales.listaFantasmas.Count)
            {
                FantasmaData fData = datosActuales.listaFantasmas[i];
                fantasmasEnEscena[i].gameObject.SetActive(fData.estaActivo);

                if (fData.estaActivo)
                {
                    fantasmasEnEscena[i].Warp(fData.posicion.ToVector3());
                }
            }
        }

        // 3. Restaurar Estado de Objetos del Escenario
        for (int i = 0; i < objetosGuardables.Count; i++)
        {
            if (objetosGuardables[i] != null)
            {
                // Buscar por ID del objeto guardado
                ObjetoEscenarioData oData = datosActuales.listaObjetosEscenario.Find(x => x.idObjeto == objetosGuardables[i].name);
                if (oData != null)
                {
                    objetosGuardables[i].SetActive(!oData.estaRecolectadoODesactivado);
                }
            }
        }

        Debug.Log("Estado del juego cargado correctamente mediante Tags.");
    }

    public void BorrarDatosGuardados()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("Partida eliminada.");
    }

    #endregion

    #region Ciclo de Vida Móvil (Android)

    private void OnApplicationQuit()
    {
        GuardarEstadoJuego();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            GuardarEstadoJuego();
        }
    }

    #endregion
}