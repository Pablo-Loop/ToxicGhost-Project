
using UnityEngine;
using UnityEngine.AI;

public class FantasmaBase : MonoBehaviour
{
    public enum EstadoFantasma
    {
        Esperando,
        Persiguiendo,
        Regresando
    }

    [Header("Referencias")]
    public Transform jugador;

    [Header("Configuracion")]
    public float radioDeteccion = 6f;
    public float distanciaMaxima = 15f;
    public float tiempoDescanso = 3f;

    protected NavMeshAgent agente;
    protected EstadoFantasma estado;

    protected Vector3 posicionInicial;
    protected Vector3 posicionAnterior;

    protected float distanciaRecorrida;
    protected float tiempoRestante;

    protected virtual void Start()
    {
        agente = GetComponent<NavMeshAgent>();

        posicionInicial = transform.position;
        posicionAnterior = transform.position;

        estado = EstadoFantasma.Esperando;
    }

    protected virtual void Update()
    {
        if (jugador == null || agente == null)
            return;

        if (!agente.isOnNavMesh)
            return;

        switch (estado)
        {
            case EstadoFantasma.Esperando:
                Esperar();
                break;

            case EstadoFantasma.Persiguiendo:
                Perseguir();
                break;

            case EstadoFantasma.Regresando:
                Regresar();
                break;
        }
    }

    protected virtual void Esperar()
    {
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            return;
        }

        float distanciaJugador = Vector3.Distance(
            transform.position,
            jugador.position
        );

        if (distanciaJugador <= radioDeteccion)
        {
            estado = EstadoFantasma.Persiguiendo;
            distanciaRecorrida = 0f;
            posicionAnterior = transform.position;

            Debug.Log("Fantasma detecto a Gasp.");
        }
    }

    protected virtual void Perseguir()
    {
        distanciaRecorrida += Vector3.Distance(
            transform.position,
            posicionAnterior
        );

        posicionAnterior = transform.position;

        if (distanciaRecorrida >= distanciaMaxima)
        {
            estado = EstadoFantasma.Regresando;
            agente.SetDestination(posicionInicial);

            Debug.Log("Fantasma termina persecucion.");
            return;
        }

        MoverHaciaJugador();
    }

    protected virtual void MoverHaciaJugador()
    {
        agente.SetDestination(jugador.position);
    }

    protected virtual void Regresar()
    {
        if (agente.pathPending)
            return;

        if (agente.remainingDistance <=
            agente.stoppingDistance + 0.1f)
        {
            agente.ResetPath();

            estado = EstadoFantasma.Esperando;
            tiempoRestante = tiempoDescanso;

            Debug.Log("Fantasma regreso a su zona.");
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            radioDeteccion
        );
    }
}
