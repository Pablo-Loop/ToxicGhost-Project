
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovimientoGaspTest : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 4f;
    public float velocidadCorrer = 7f;

    [Header("Gravedad")]
    public float gravedad = -9.81f;

    private CharacterController controlador;
    private float velocidadVertical;

    void Start()
    {
        controlador = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direccion = new Vector3(
            horizontal,
            0f,
            vertical
        ).normalized;

        float velocidadActual = Input.GetKey(KeyCode.LeftShift)
            ? velocidadCorrer
            : velocidadCaminar;

        if (controlador.isGrounded && velocidadVertical < 0f)
        {
            velocidadVertical = -2f;
        }

        velocidadVertical += gravedad * Time.deltaTime;

        Vector3 movimiento = direccion * velocidadActual;
        movimiento.y = velocidadVertical;

        controlador.Move(movimiento * Time.deltaTime);

        if (direccion.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direccion);
        }
    }
}
