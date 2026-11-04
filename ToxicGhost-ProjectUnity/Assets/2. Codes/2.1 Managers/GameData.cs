using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PosicionData
{
    public float x, y, z;

    public PosicionData(Vector3 pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class FantasmaData
{
    public string idFantasma;
    public PosicionData posicion;
    public bool estaActivo;
}

[Serializable]
public class ObjetoEscenarioData
{
    public string idObjeto;
    public bool estaRecolectadoODesactivado;
}

[Serializable]
public class GameData
{
    public PosicionData posicionJugador;
    public int progresoHistoria = 0;
    public List<FantasmaData> listaFantasmas = new List<FantasmaData>();
    public List<ObjetoEscenarioData> listaObjetosEscenario = new List<ObjetoEscenarioData>();
}