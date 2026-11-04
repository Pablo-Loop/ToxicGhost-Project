using UnityEngine;

public class RestartCheckPoint : MonoBehaviour
{
    [Tooltip("Si se marca, el checkpoint solo se guardará la primera vez que Gasp pase por él.")]
    [SerializeField] private bool soloUnaVez = false;

    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado && soloUnaVez) return;

        if (other.CompareTag("Player"))
        {
            activado = true;

            if (GameManagerMain.Instance != null)
            {
                // Guarda la posición actual de este Checkpoint como el punto de Respawn de Gasp
                GameManagerMain.Instance.RegistrarCheckpoint(transform.position);
            }
        }
    }
}
