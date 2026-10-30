using UnityEngine;

public class FantasmaQTE : MonoBehaviour
{
    public float velocidad = 1.5f;
    public float distanciaScreamer = 1.5f;

    private Transform jugador;
    private bool acercandose = false;

    public void IniciarPersecucion(Transform objetivo)
    {
        jugador = objetivo;
        acercandose = true;
    }

    void Update()
    {
        if (!acercandose || jugador == null)
            return;

        // Calculamos la dirección hacia el jugador
        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0f;

        // Si está suficientemente cerca, se detiene
        if (direccion.magnitude <= distanciaScreamer)
        {
            acercandose = false;
            return;
        }

        // Avanza hacia el jugador
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(jugador.position.x, transform.position.y, jugador.position.z),
            velocidad * Time.deltaTime
        );
    }

    public bool EstaCercaDelJugador()
    {
        if (jugador == null)
            return false;

        Vector3 diferencia = jugador.position - transform.position;
        diferencia.y = 0f;

        return diferencia.magnitude <= distanciaScreamer;
    }

    public void Detener()
    {
        acercandose = false;
    }
}
