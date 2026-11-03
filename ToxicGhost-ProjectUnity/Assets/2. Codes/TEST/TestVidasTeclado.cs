
using UnityEngine;
using UnityEngine.InputSystem;

public class TestVidasTeclado : MonoBehaviour
{
    [SerializeField] private LivesSystem livesSystem;

    void Start()
    {
        Debug.Log("TEST VIDAS: Script iniciado");
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.kKey.wasPressedThisFrame)
        {
            Debug.Log("TEST VIDAS: Tecla K detectada");

            if (livesSystem != null)
            {
                livesSystem.TakeDamage(1);
                Debug.Log("TEST VIDAS: Vida descontada");
            }
        }
    }
}

