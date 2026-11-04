using UnityEngine;

public class LivePickUp : MonoBehaviour
{
    [Header("Configuración de Curación")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private LivesSystem livesSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            if (livesSystem != null)
            {
                // Intentar curar. Si fue exitoso (no tenía vida máxima), destruye el objeto.
                bool healed = livesSystem.Heal(healAmount);

                if (healed)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}