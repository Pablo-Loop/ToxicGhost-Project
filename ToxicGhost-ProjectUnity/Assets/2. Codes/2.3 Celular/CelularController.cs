
using UnityEngine;

public class CelularController : MonoBehaviour
{
    public GameObject panelTelefono;

    public void AlternarCelular()
    {
        if (panelTelefono == null) return;

        panelTelefono.SetActive(!panelTelefono.activeSelf);
    }

    public void AbrirCelular()
    {
        if (panelTelefono != null)
            panelTelefono.SetActive(true);
    }

    public void CerrarCelular()
    {
        if (panelTelefono != null)
            panelTelefono.SetActive(false);
    }
}
