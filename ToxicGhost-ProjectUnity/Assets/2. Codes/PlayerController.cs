using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Animator animator;
    [SerializeField] private string runParamName = "isRunning";

    [Tooltip("Asigna la cámara que muestra el juego.")]
    [SerializeField] private Transform movementReference;

    [SerializeField] private LayerMask groundLayers;

    [Header("Movimiento")]
    [SerializeField, Min(0f)] private float moveSpeed = 6f;
    [SerializeField, Min(0f)] private float acceleration = 35f;
    [SerializeField, Min(0f)] private float rotationSpeed = 720f;

    [SerializeField, Range(0f, 1f)]
    private float airControl = 0.6f;

    [Tooltip("Entrada mínima del joystick para iniciar el movimiento.")]
    [SerializeField, Range(0.01f, 0.5f)]
    private float movementDeadZone = 0.15f;

    [Header("Salto")]
    [SerializeField, Min(0.1f)] private float jumpHeight = 1.8f;
    [SerializeField, Min(0f)] private float jumpBuffer = 0.12f;

    [Header("Detección del suelo")]
    [SerializeField, Min(0.01f)] private float groundRadius = 0.18f;
    [SerializeField, Min(0.01f)] private float groundDistance = 0.12f;

    [SerializeField, Range(0f, 80f)]
    private float maxGroundAngle = 50f;

    private Rigidbody body;
    private Vector2 moveInput;

    // Conserva una referencia válida si la cámara apunta
    // exactamente hacia arriba o abajo.
    private Vector3 lastValidForward = Vector3.forward;

    private float jumpRequestedAt = float.NegativeInfinity;

    public bool IsGrounded { get; private set; }

    // Compatibilidad con Unity 6 y versiones anteriores.
    private Vector3 Velocity
    {
        get
        {
#if UNITY_6000_0_OR_NEWER
            return body.linearVelocity;
#else
            return body.velocity;
#endif
        }
        set
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = value;
#else
            body.velocity = value;
#endif
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (movementReference == null && Camera.main != null)
            movementReference = Camera.main.transform;

        if (groundCheck == null)
        {
            Debug.LogError(
                "PlayerController: asigna GroundCheck.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        ReadInput();

        if (moveInput.sqrMagnitude <=
            movementDeadZone * movementDeadZone)
        {
            moveInput = Vector2.zero;
        }

        UpdateAnimations();
    }

    private void ReadInput()
    {
        Gamepad pad = Gamepad.current;

        moveInput = pad != null
            ? pad.leftStick.ReadValue()
            : Vector2.zero;

        if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            RequestJump();

        // Controles de prueba para el editor.
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            Vector2 keyboardInput = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) -
                (keyboard.aKey.isPressed ? 1f : 0f),

                (keyboard.wKey.isPressed ? 1f : 0f) -
                (keyboard.sKey.isPressed ? 1f : 0f)
            );

            if (keyboardInput.sqrMagnitude > 0f)
                moveInput = keyboardInput;

            if (keyboard.spaceKey.wasPressedThisFrame)
                RequestJump();
        }

        // Evita que las diagonales sean más rápidas.
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private Vector3 GetMovementDirection()
    {
        if (movementReference != null)
        {
            // Elimina la inclinación vertical de la cámara:
            // mirar arriba no hace que el jugador vuele.
            Vector3 cameraForward = Vector3.ProjectOnPlane(
                movementReference.forward,
                Vector3.up
            );

            if (cameraForward.sqrMagnitude > 0.001f)
                lastValidForward = cameraForward.normalized;
        }

        Vector3 right = Vector3.Cross(
            Vector3.up,
            lastValidForward
        );

        return
            lastValidForward * moveInput.y +
            right * moveInput.x;
    }

    private void FixedUpdate()
    {

        body.angularVelocity = Vector3.zero;

        Vector3 velocity = Velocity;

        IsGrounded = CheckGround() && velocity.y <= 0.1f;

        // Actualiza la dirección con la cámara continuamente.
        Vector3 direction = GetMovementDirection();

        Vector3 horizontalVelocity =
            new Vector3(velocity.x, 0f, velocity.z);

        float control = IsGrounded ? 1f : airControl;

        // La aceleración suaviza los cambios de rumbo.
        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            direction * moveSpeed,
            acceleration * control * Time.fixedDeltaTime
        );

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        bool hasBufferedJump =
            Time.time - jumpRequestedAt <= jumpBuffer;

        if (hasBufferedJump && IsGrounded)
        {
            velocity.y = Mathf.Sqrt(
                2f * Mathf.Abs(Physics.gravity.y) * jumpHeight
            );

            jumpRequestedAt = float.NegativeInfinity;
            IsGrounded = false;
        }

        Velocity = velocity;

        // Sin entrada de movimiento, girar la cámara
        // no cambia la orientación del personaje.
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction, Vector3.up);

            body.MoveRotation(
                Quaternion.RotateTowards(
                    body.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                )
            );
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null ||
            string.IsNullOrEmpty(runParamName))
        {
            return;
        }

        bool hasInput = moveInput.sqrMagnitude > 0.001f;
        bool isRunning = hasInput && IsGrounded;

        animator.SetBool(runParamName, isRunning);
    }

    private bool CheckGround()
    {
        Vector3 origin = groundCheck.position +
            Vector3.up * (groundRadius + 0.05f);

        if (!Physics.SphereCast(
                origin,
                groundRadius,
                Vector3.down,
                out RaycastHit hit,
                groundDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return Vector3.Angle(hit.normal, Vector3.up)
            <= maxGroundAngle;
    }

    public void RequestJump()
    {
        if (isActiveAndEnabled)
            jumpRequestedAt = Time.time;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        jumpRequestedAt = float.NegativeInfinity;
        IsGrounded = false;

        // Evita mantener la animación de carrera
        // cuando otra mecánica desactiva el controlador.
        if (animator != null &&
            !string.IsNullOrEmpty(runParamName))
        {
            animator.SetBool(runParamName, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Vector3 origin = groundCheck.position +
            Vector3.up * (groundRadius + 0.05f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, groundRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            origin + Vector3.down * groundDistance,
            groundRadius
        );
    }
}