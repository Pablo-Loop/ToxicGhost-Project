using UnityEngine;

public class TriggerHojaUI : MonoBehaviour
{
    [Header("Configuración del Trigger")]
    [Tooltip("Índice de la hoja a abrir en UImanager: 1 = Hoja2, 2 = Hoja3, 3 = Hoja4, etc.")]
    [SerializeField] private int indiceDeHoja = 1;

    private bool yaActivado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (yaActivado) return;

        if (other.CompareTag("Player"))
        {
            yaActivado = true;

            if (UImanager.Instance != null)
            {
                UImanager.Instance.MostrarHojaPorIndice(indiceDeHoja);
            }
            else
            {
                UImanager manager = FindFirstObjectByType<UImanager>();
                if (manager != null)
                {
                    manager.MostrarHojaPorIndice(indiceDeHoja);
                }
            }

            // Desactivar el trigger para evitar re-activaciones
            gameObject.SetActive(false);
        }
    }
}