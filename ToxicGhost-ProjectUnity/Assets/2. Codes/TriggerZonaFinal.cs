using UnityEngine;

public class TriggerZonaFinal : MonoBehaviour
{
    [SerializeField] private bool soloUnaVez = true;
    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado && soloUnaVez) return;

        if (other.CompareTag("Player"))
        {
            activado = true;

            if (AudioManager.Instance != null)
            {
                // Usa CambiarAMusicaZonaFinal() o TransicionarAMusicaZonaFinal()
                AudioManager.Instance.CambiarAMusicaZonaFinal();
            }

            Debug.Log("Jugador ha entrado a la Zona Final. Música cambiada.");
        }
    }
}
