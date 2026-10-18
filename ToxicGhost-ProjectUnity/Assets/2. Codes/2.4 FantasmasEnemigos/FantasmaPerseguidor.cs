
using UnityEngine;

public class FantasmaPerseguidor : FantasmaBase
{
    [Header("Perseguidor")]
    public float velocidadPersecucion = 3.5f;
    public float velocidadRegreso = 2f;

    protected override void Perseguir()
    {
        agente.speed = velocidadPersecucion;

        base.Perseguir();
    }

    protected override void Regresar()
    {
        agente.speed = velocidadRegreso;

        base.Regresar();
    }
}

