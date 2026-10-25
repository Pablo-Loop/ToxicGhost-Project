using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Opcional: eje del nivel. No asignar la cámara.")]
    [SerializeField] private Transform movementReference;

    [SerializeField] private LayerMask groundLayers;

    [Header("Movimiento")]
    [SerializeField, Min(0f)] private float moveSpeed = 6f;
    [SerializeField, Min(0f)] private float acceleration = 35f;
    [SerializeField, Min(0f)] private float rotationSpeed = 720f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.6f;

    [Header("Salto")]
    [SerializeField, Min(0.1f)] private float jumpHeight = 1.8f;
    [SerializeField, Min(0f)] private float jumpBuffer = 0.12f;

    [Header("Suelo")]
    [SerializeField, Min(0.01f)] private float groundRadius = 0.18f;
    [SerializeField, Min(0.01f)] private float groundDistance = 0.12f;
    [SerializeField, Range(0f, 80f)] private float maxGroundAngle = 50f;

    private Rigidbody body;
    private Vector2 moveInput;
    private float jumpRequestedAt = float.NegativeInfinity;

    public bool IsGrounded { get; private set; }

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

        if (groundCheck == null)
        {
            Debug.LogError("Falta asignar GroundCheck.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        // El joystick en pantalla genera un Gamepad virtual.
        Gamepad pad = Gamepad.current;

        moveInput = pad != null
            ? pad.leftStick.ReadValue()
            : Vector2.zero;

        if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            RequestJump();

        // Controles para probar en el editor.
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            Vector2 keys = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) -
                (keyboard.aKey.isPressed ? 1f : 0f),

                (keyboard.wKey.isPressed ? 1f : 0f) -
                (keyboard.sKey.isPressed ? 1f : 0f)
            );

            if (keys.sqrMagnitude > 0f)
                moveInput = keys;

            if (keyboard.spaceKey.wasPressedThisFrame)
                RequestJump();
        }

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void FixedUpdate()
    {
        Vector3 velocity = Velocity;
        IsGrounded = CheckGround() && velocity.y <= 0.1f;

        // Referencia independiente de la cámara.
        Vector3 forward = Vector3.forward;

        if (movementReference != null)
        {
            forward = Vector3.ProjectOnPlane(
                movementReference.forward,
                Vector3.up
            ).normalized;

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward);

        Vector3 direction =
            forward * moveInput.y +
            right * moveInput.x;

        Vector3 horizontal =
            new Vector3(velocity.x, 0f, velocity.z);

        float control = IsGrounded ? 1f : airControl;

        horizontal = Vector3.MoveTowards(
            horizontal,
            direction * moveSpeed,
            acceleration * control * Time.fixedDeltaTime
        );

        velocity.x = horizontal.x;
        velocity.z = horizontal.z;

        bool wantsJump =
            Time.time - jumpRequestedAt <= jumpBuffer;

        if (wantsJump && IsGrounded)
        {
            velocity.y = Mathf.Sqrt(
                2f * Mathf.Abs(Physics.gravity.y) * jumpHeight
            );

            jumpRequestedAt = float.NegativeInfinity;
            IsGrounded = false;
        }

        Velocity = velocity;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(direction, Vector3.up);

            body.MoveRotation(
                Quaternion.RotateTowards(
                    body.rotation,
                    desiredRotation,
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
        jumpRequestedAt = Time.time;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        jumpRequestedAt = float.NegativeInfinity;
    }
}