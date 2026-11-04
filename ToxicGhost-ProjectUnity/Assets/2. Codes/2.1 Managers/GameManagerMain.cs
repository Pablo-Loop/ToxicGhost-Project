using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManagerMain : MonoBehaviour
{
    public static GameManagerMain Instance { get; private set; }

    [Header("Referencias Principales")]
    [Tooltip("Asigna únicamente el Transform del Jugador (Gasp).")]
    [SerializeField] private Transform jugadorTransform;

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

    private void InicializarReferenciasPorTags()
    {
        GameObject[] encontrados = GameObject.FindGameObjectsWithTag("Saveable");
        objetosGuardables.Clear();

        foreach (GameObject obj in encontrados)
        {
            if (obj.name != "Escena" && obj.GetComponent<Cinematic>() == null)
            {
                objetosGuardables.Add(obj);
            }
        }

        GameObject[] fantasmasObj = GameObject.FindGameObjectsWithTag("Player");
        if (fantasmasObj.Length == 0)
        {
            fantasmasObj = GameObject.FindGameObjectsWithTag("Player");
        }

        fantasmasEnEscena.Clear();

        foreach (GameObject g in fantasmasObj)
        {
            if (g.transform != jugadorTransform)
            {
                NavMeshAgent agent = g.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    fantasmasEnEscena.Add(agent);
                }
            }
        }
    }

    #region Sistema de Checkpoints y Respawn

    /// <summary>
    /// Se llama automáticamente cuando Gasp cruza el trigger de un Checkpoint.
    /// </summary>
    public void RegistrarCheckpoint(Vector3 posicionCheckpoint)
    {
        datosActuales.posicionJugador = new PosicionData(posicionCheckpoint);
        GuardarEstadoJuego();
        Debug.Log($"Checkpoint guardado en la posición: {posicionCheckpoint}");
    }

    /// <summary>
    /// Reposiciona a Gasp en el último checkpoint sin reiniciar la escena.
    /// </summary>
    public void RespawnEnUltimoCheckpoint()
    {
        if (jugadorTransform == null) return;

        // Si tenemos guardada una posición previa de Checkpoint
        if (datosActuales.posicionJugador != null)
        {
            Vector3 targetPos = datosActuales.posicionJugador.ToVector3();
            targetPos.y += 0.1f; // Holgura para evitar colisionar con el piso

            // Desactivar temporalmente CharacterController o Rigidbody para poder mover a Gasp
            CharacterController cc = jugadorTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            jugadorTransform.position = targetPos;

            if (cc != null) cc.enabled = true;
        }

        Debug.Log("Gasp ha reaparecido en el último checkpoint guardado.");
    }

    #endregion

    #region Guardado y Cargado

    public void GuardarEstadoJuego()
    {
        if (jugadorTransform == null) return;

        // 1. Guardar Jugador
        if (datosActuales.posicionJugador == null)
        {
            datosActuales.posicionJugador = new PosicionData(jugadorTransform.position);
        }

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

        // 3. Guardar Objetos del Escenario
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

        // 4. Guardar JSON en PlayerPrefs
        string json = JsonUtility.ToJson(datosActuales);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void CargarEstadoJuego()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        datosActuales = JsonUtility.FromJson<GameData>(json);

        // 1. Restaurar Posición del Jugador
        RespawnEnUltimoCheckpoint();

        // 2. Restaurar Fantasmas
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

        // 3. Restaurar Objetos del Escenario
        for (int i = 0; i < objetosGuardables.Count; i++)
        {
            if (objetosGuardables[i] != null)
            {
                ObjetoEscenarioData oData = datosActuales.listaObjetosEscenario.Find(x => x.idObjeto == objetosGuardables[i].name);
                if (oData != null)
                {
                    objetosGuardables[i].SetActive(!oData.estaRecolectadoODesactivado);
                }
            }
        }
    }

    #endregion

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
}