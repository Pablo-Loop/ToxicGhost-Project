using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Clue : MonoBehaviour
{
    [Header("REFERENCIAS")]
    [SerializeField] private GameObject visualClue;
    [SerializeField] private GameObject unlockingTree;

    [Header("CONFIGURACIÓN")]
    [SerializeField] private float displayDuration = 10f;

    private bool playerIsNearby = false;
    private bool isShowingClue = false;

    private void Start()
    {
        if (visualClue != null)
        {
            visualClue.SetActive(false);
        }
    }

    private void Update()
    {
        // Verificamos el teclado (para pruebas en el Editor) y el touch/gamepad en dispositivos móviles
        Keyboard keyboard = Keyboard.current;
        bool interactPressed = (keyboard != null && keyboard.eKey.wasPressedThisFrame);

        if (playerIsNearby && !isShowingClue && interactPressed)
        {
            ClueActivate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = true;
            Debug.Log("Jugador cerca de la pista. ¡Presiona 'E' (o interactúa) para ver la pista!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = false;
        }
    }

    private void ClueActivate()
    {
        // 1. Reproducir el efecto de sonido de la pista
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirHintSound();
        }

        // 2. Destruir obstáculo y mostrar la pista visual
        Destroy(unlockingTree);
        StartCoroutine(ShowClueRoutine());
    }

    private IEnumerator ShowClueRoutine()
    {
        isShowingClue = true;

        visualClue.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        visualClue.SetActive(false);

        isShowingClue = false;
    }
}