using UnityEngine;

public class SpikesSpawn : MonoBehaviour
{
    [Header("Configuraci�n de Da�o")]
    [SerializeField] private int damageAmount = 1;

    [Header("Punto de Reaparici�n")]
    [SerializeField] private Transform checkpointTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Restar vida al jugador
            LivesSystem playerHealth = other.GetComponent<LivesSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }

            // 2. Teletransportar al Checkpoint
            TeleportToCheckpoint(other.gameObject);
        }
    }

    private void TeleportToCheckpoint(GameObject playerObj)
    {
        if (checkpointTarget == null)
        {
            Debug.LogWarning("Spikes: No se ha asignado un Checkpoint en el Inspector.", this);
            return;
        }

        // Si el jugador utiliza Rigidbody, reiniciamos su velocidad f�sica antes de moverlo
        if (playerObj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = checkpointTarget.position;
            rb.rotation = checkpointTarget.rotation;
        }
        else
        {
            playerObj.transform.position = checkpointTarget.position;
            playerObj.transform.rotation = checkpointTarget.rotation;
        }
    }
}