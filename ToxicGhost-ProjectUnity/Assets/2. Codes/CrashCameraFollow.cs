using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CrashCameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Tooltip("Punto al que mira, relativo a la posición del jugador.")]
    [SerializeField]
    private Vector3 targetOffset =
        new Vector3(0f, 0.5f, 0f);

    [Header("Órbita")]
    [SerializeField, Min(0.1f)] private float distance = 5f;
    [SerializeField] private float initialYaw = 0f;
    [SerializeField] private float initialPitch = 18f;

    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 60f;

    [Tooltip("Grados por arrastre de un ancho/alto corto de pantalla.")]
    [SerializeField, Min(1f)] private float touchSensitivity = 180f;

    [SerializeField, Min(0.01f)] private float orbitSmoothTime = 0.08f;
    [SerializeField] private bool invertVertical;

    [Header("Obstáculos")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField, Min(0.01f)] private float collisionRadius = 0.3f;
    [SerializeField, Min(0f)] private float collisionPadding = 0.08f;
    [SerializeField, Min(0.01f)] private float returnSpeed = 8f;

    private float desiredYaw;
    private float desiredPitch;

    private float currentYaw;
    private float currentPitch;

    private float yawVelocity;
    private float pitchVelocity;
    private float currentDistance;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Falta asignar Target a la cámara.", this);
            enabled = false;
            return;
        }

        ResetView();
    }

    // Recibe desplazamiento del dedo en píxeles.
    public void AddLookDelta(Vector2 screenDelta)
    {
        float screenSize = Mathf.Max(
            1f,
            Mathf.Min(Screen.width, Screen.height)
        );

        Vector2 normalizedDelta = screenDelta / screenSize;

        desiredYaw += normalizedDelta.x * touchSensitivity;

        float verticalSign = invertVertical ? 1f : -1f;

        desiredPitch = Mathf.Clamp(
            desiredPitch +
            normalizedDelta.y * touchSensitivity * verticalSign,
            minPitch,
            maxPitch
        );
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            desiredYaw,
            ref yawVelocity,
            orbitSmoothTime
        );

        currentPitch = Mathf.SmoothDampAngle(
            currentPitch,
            desiredPitch,
            ref pitchVelocity,
            orbitSmoothTime
        );

        Quaternion orbit =
            Quaternion.Euler(currentPitch, currentYaw, 0f);

        // Usa la posición visual interpolada del Rigidbody.
        // Sigue también la altura del salto.
        Vector3 pivot = target.position + targetOffset;
        Vector3 backward = orbit * Vector3.back;

        float allowedDistance = distance;

        if (Physics.SphereCast(
                pivot,
                collisionRadius,
                backward,
                out RaycastHit hit,
                distance,
                obstacleLayers,
                QueryTriggerInteraction.Ignore))
        {
            allowedDistance = Mathf.Max(
                0.01f,
                hit.distance - collisionPadding
            );
        }

        // Acercamiento inmediato para no suavizar a través de una pared.
        // Regreso progresivo cuando vuelve a haber espacio.
        if (allowedDistance < currentDistance)
        {
            currentDistance = allowedDistance;
        }
        else
        {
            float blend = 1f - Mathf.Exp(-returnSpeed * Time.deltaTime);

            currentDistance = Mathf.Lerp(
                currentDistance,
                allowedDistance,
                blend
            );
        }

        transform.position = pivot + backward * currentDistance;

        // El jugador permanece como centro del encuadre.
        transform.rotation = Quaternion.LookRotation(
            pivot - transform.position,
            Vector3.up
        );
    }

    public void ResetView()
    {
        desiredYaw = currentYaw = initialYaw;

        desiredPitch = currentPitch =
            Mathf.Clamp(initialPitch, minPitch, maxPitch);

        yawVelocity = 0f;
        pitchVelocity = 0f;
        currentDistance = distance;
    }
}