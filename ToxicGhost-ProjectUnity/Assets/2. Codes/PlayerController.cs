using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Asigna Main Camera.")]
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

    private bool movementStarted;
    private Vector3 lockedForward = Vector3.forward;

    private float jumpRequestedAt = float.NegativeInfinity;

    public bool IsGrounded { get; private set; }

    // Compatibilidad con Unity 6 y Unity 2022.
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

        bool hasInput =
            moveInput.sqrMagnitude >
            movementDeadZone * movementDeadZone;

        if (!hasInput)
        {
            moveInput = Vector2.zero;
            movementStarted = false;
            return;
        }

        // Captura la cámara al comenzar cada desplazamiento.
        // No vuelve a capturarla mientras el joystick siga activo.
        if (!movementStarted)
        {
            CaptureMovementReference();
            movementStarted = true;
        }
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

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void CaptureMovementReference()
    {
        lockedForward = Vector3.forward;

        if (movementReference == null)
            return;

        Vector3 cameraForward = Vector3.ProjectOnPlane(
            movementReference.forward,
            Vector3.up
        );

        if (cameraForward.sqrMagnitude > 0.001f)
            lockedForward = cameraForward.normalized;
    }

    private void FixedUpdate()
    {
        Vector3 velocity = Velocity;

        IsGrounded = CheckGround() && velocity.y <= 0.1f;

        Vector3 lockedRight =
            Vector3.Cross(Vector3.up, lockedForward);

        Vector3 direction =
            lockedForward * moveInput.y +
            lockedRight * moveInput.x;

        Vector3 horizontalVelocity =
            new Vector3(velocity.x, 0f, velocity.z);

        float control = IsGrounded ? 1f : airControl;

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

        // Solo el movimiento orienta al personaje.
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
        movementStarted = false;
        jumpRequestedAt = float.NegativeInfinity;
        IsGrounded = false;
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