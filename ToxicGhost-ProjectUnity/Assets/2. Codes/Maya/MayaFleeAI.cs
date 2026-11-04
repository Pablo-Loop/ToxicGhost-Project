using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class MayaFleeAI : MonoBehaviour
{
    private enum State
    {
        Waiting,
        Fleeing,
        Finished
    }

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSightTarget;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private PhoneState phone;

    [Header("Puntos de escape")]
    [SerializeField] private Transform[] escapePoints;

    [Tooltip("Distancia para considerar que llegó a un punto.")]
    [SerializeField, Min(0.1f)]
    private float arrivalDistance = 0.4f;

    [Tooltip("Desplazamiento mínimo para elegir un nuevo destino.")]
    [SerializeField, Min(0.1f)]
    private float minimumPointDistance = 1f;

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runningParameter = "isRunning";

    [Header("Detección")]
    [SerializeField, Min(0.1f)]
    private float alertRadius = 8f;

    [Tooltip("Solo suelo y obstáculos. Excluir Player y Maya.")]
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Huida")]
    [SerializeField, Min(0.1f)]
    private float fleeSpeed = 4f;

    [SerializeField, Min(0.1f)]
    private float decisionInterval = 0.6f;

    [Tooltip("Si no avanza durante este tiempo, busca otro punto.")]
    [SerializeField, Min(0.5f)]
    private float stuckTimeout = 2f;

    [Header("Entrega")]
    [SerializeField, Min(0.1f)]
    private float returnDistance = 2f;

    [SerializeField, Min(0.1f)]
    private float maxInteractionHeight = 2f;

    [Header("Eventos")]
    public UnityEvent onStartedFleeing = new UnityEvent();
    public UnityEvent onPhoneReturned = new UnityEvent();

    private NavMeshAgent agent;
    private NavMeshPath reusablePath;

    private State state = State.Waiting;

    private float nextDecision;
    private int currentPointIndex = -1;

    private Vector3 lastProgressPosition;
    private float lastProgressTime;

    private bool AgentReady =>
        agent != null &&
        agent.isActiveAndEnabled &&
        agent.isOnNavMesh;

    private Vector3 PlayerSightPosition =>
        playerSightTarget != null
            ? playerSightTarget.position
            : player.position + Vector3.up;

    private Vector3 MayaSightPosition =>
        interactionPoint != null
            ? interactionPoint.position
            : transform.position + Vector3.up;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        reusablePath = new NavMeshPath();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (player == null || phone == null ||
            escapePoints == null || escapePoints.Length == 0)
        {
            Debug.LogError(
                "MayaFleeAI: asigna Player, Phone " +
                "y los puntos de escape.",
                this
            );

            enabled = false;
            return;
        }

        if (!AgentReady)
        {
            Debug.LogError(
                "MayaFleeAI: Maya debe comenzar sobre un NavMesh " +
                "compatible con su Agent Type.",
                this
            );

            enabled = false;
            return;
        }

        agent.speed = fleeSpeed;

        // Garantiza que pueda entrar en el radio de llegada.
        agent.stoppingDistance =
            Mathf.Min(0.1f, arrivalDistance);

        agent.ResetPath();
        agent.isStopped = true;

        ResetProgressTracking();

        if (phone.IsDelivered)
            FinishMovement();

        UpdateAnimation();
    }

    private void Update()
    {
        if (!AgentReady)
            return;

        if (phone.IsDelivered)
        {
            if (state != State.Finished)
                FinishMovement();

            UpdateAnimation();
            return;
        }

        if (state == State.Fleeing)
            TrackProgress();

        if (Time.time >= nextDecision)
        {
            nextDecision = Time.time + decisionInterval;

            if (state == State.Waiting && CanDetectPlayer())
            {
                state = State.Fleeing;
                agent.isStopped = false;

                ResetProgressTracking();
                onStartedFleeing.Invoke();

                ChooseEscapePoint(false);
            }
            else if (state == State.Fleeing)
            {
                UpdateEscapeRoute();
            }
        }

        UpdateAnimation();
    }

    private bool CanDetectPlayer()
    {
        Vector3 difference =
            PlayerSightPosition - MayaSightPosition;

        return difference.sqrMagnitude <= alertRadius * alertRadius &&
               HasClearView();
    }

    private bool HasClearView()
    {
        return !Physics.Linecast(
            MayaSightPosition,
            PlayerSightPosition,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    private void UpdateEscapeRoute()
    {
        if (agent.pathPending)
            return;

        bool invalidPath =
            !agent.hasPath ||
            agent.pathStatus != NavMeshPathStatus.PathComplete;

        bool arrived =
            agent.hasPath &&
            agent.remainingDistance <= arrivalDistance;

        bool stuck =
            Time.time - lastProgressTime >= stuckTimeout;

        if (invalidPath || arrived || stuck)
        {
            // Si se atasca, intenta primero un destino diferente.
            bool selected = ChooseEscapePoint(stuck);

            if (!selected && stuck)
                ChooseEscapePoint(false);
        }
    }

    private bool ChooseEscapePoint(bool excludeCurrent)
    {
        int bestIndex = -1;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < escapePoints.Length; i++)
        {
            Transform point = escapePoints[i];

            if (point == null)
                continue;

            if (excludeCurrent && i == currentPointIndex)
                continue;

            Vector3 fromMaya = point.position - transform.position;
            fromMaya.y = 0f;

            if (fromMaya.sqrMagnitude <
                minimumPointDistance * minimumPointDistance)
            {
                continue;
            }

            // Solo acepta puntos alcanzables por este agente.
            if (!agent.CalculatePath(
                    point.position,
                    reusablePath) ||
                reusablePath.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            Vector3 fromPlayer = point.position - player.position;
            fromPlayer.y = 0f;

            // Prefiere el punto más alejado de Gasp.
            float score = fromPlayer.sqrMagnitude;

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        if (bestIndex < 0)
            return false;

        if (!agent.CalculatePath(
                escapePoints[bestIndex].position,
                reusablePath) ||
            reusablePath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        agent.isStopped = false;

        if (!agent.SetPath(reusablePath))
            return false;

        currentPointIndex = bestIndex;
        ResetProgressTracking();

        return true;
    }

    private void TrackProgress()
    {
        Vector3 displacement =
            transform.position - lastProgressPosition;

        displacement.y = 0f;

        if (displacement.sqrMagnitude >= 0.04f)
            ResetProgressTracking();
    }

    private void ResetProgressTracking()
    {
        lastProgressPosition = transform.position;
        lastProgressTime = Time.time;
    }

    public bool CanReturnPhone()
    {
        if (!isActiveAndEnabled ||
            !AgentReady ||
            player == null ||
            phone == null ||
            phone.IsDelivered ||
            !phone.HasPhone ||
            state != State.Fleeing)
        {
            return false;
        }

        Vector3 difference =
            player.position - transform.position;

        if (Mathf.Abs(difference.y) > maxInteractionHeight)
            return false;

        difference.y = 0f;

        return difference.sqrMagnitude <=
               returnDistance * returnDistance &&
               HasClearView();
    }

    public bool TryReturnPhone()
    {
        if (!CanReturnPhone())
            return false;

        if (!phone.TryDeliver())
            return false;

        FinishMovement();
        onPhoneReturned.Invoke();

        return true;
    }

    private void FinishMovement()
    {
        state = State.Finished;

        if (AgentReady)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        SetRunning(false);
    }

    private void UpdateAnimation()
    {
        bool moving =
            AgentReady &&
            !agent.isStopped &&
            agent.velocity.sqrMagnitude > 0.05f;

        SetRunning(moving);
    }

    private void SetRunning(bool running)
    {
        if (animator != null &&
            !string.IsNullOrEmpty(runningParameter))
        {
            animator.SetBool(runningParameter, running);
        }
    }

    private void OnDisable()
    {
        if (AgentReady)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        SetRunning(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, returnDistance);

        if (escapePoints == null)
            return;

        Gizmos.color = Color.cyan;

        foreach (Transform point in escapePoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawWireSphere(point.position, 0.25f);
        }
    }
}