using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostThiefAI : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Chase,
        Flee,
        Rest
    }

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSightTarget;
    [SerializeField] private Transform eyes;
    [SerializeField] private PhoneState phone;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string spotPlayerParam = "spotPlayer";
    [SerializeField] private string isRunningParam = "isRunning";

    [Header("Visión")]
    [Tooltip("Solo capas que bloquean la vista. Excluir jugador y enemigo.")]
    [SerializeField] private LayerMask obstacleLayers;

    [SerializeField, Min(0.1f)] private float visionRange = 9f;
    [SerializeField, Range(1f, 180f)] private float visionAngle = 90f;
    [SerializeField, Min(0f)] private float loseSightDelay = 1.5f;

    [Header("Velocidades")]
    [SerializeField, Min(0f)] private float patrolSpeed = 2f;
    [SerializeField, Min(0f)] private float chaseSpeed = 4f;
    [SerializeField, Min(0f)] private float fleeSpeed = 4.5f;

    [Header("Interacción")]
    [SerializeField, Min(0.1f)] private float stealDistance = 1.8f;
    [SerializeField, Min(0.1f)] private float recoverDistance = 2f;

    [Tooltip("Diferencia de altura máxima entre los pivotes.")]
    [SerializeField, Min(0.1f)]
    private float maxInteractionHeight = 2f;

    [SerializeField, Min(0f)] private float restDuration = 4f;

    [Header("Huida")]
    [SerializeField, Min(1f)] private float fleeStep = 6f;
    [SerializeField, Min(0.1f)] private float thinkInterval = 0.2f;

    [Header("Screamer")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamerClip;

    [Header("Diagnóstico")]
    [SerializeField] private bool showDebugMessages;

    private NavMeshAgent agent;
    private NavMeshPath reusablePath;

    private State state = State.Patrol;

    private int patrolIndex;
    private float nextThink;
    private float lastSeenAt;
    private float restUntil;

    private Vector3 SightTarget =>
        playerSightTarget != null
            ? playerSightTarget.position
            : player.position + Vector3.up;

    private bool AgentReady =>
        agent != null &&
        agent.isActiveAndEnabled &&
        agent.isOnNavMesh;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        reusablePath = new NavMeshPath();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (player == null || eyes == null || phone == null ||
            patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError(
                "GhostThiefAI: asigna Player, Eyes, Phone " +
                "y al menos un punto de patrulla.",
                this
            );

            enabled = false;
            return;
        }

        if (!AgentReady)
        {
            Debug.LogError(
                "GhostThiefAI: el enemigo debe comenzar " +
                "sobre un NavMesh compatible con su agente.",
                this
            );

            enabled = false;
            return;
        }

        // Distancia pequeña para los destinos de patrulla y huida.
        agent.stoppingDistance = 0.1f;

        agent.speed = patrolSpeed;
        agent.isStopped = false;

        GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (!AgentReady)
            return;

        // El robo se comprueba cada frame, antes del temporizador.
        if (state == State.Chase &&
            phone.HasPhone &&
            IsWithinInteractionRange(stealDistance) &&
            HasClearView())
        {
            StealPhone();
            UpdateAnimations();
            return;
        }

        // Las búsquedas de rutas siguen limitadas por intervalo.
        if (Time.time >= nextThink)
        {
            nextThink = Time.time + thinkInterval;

            switch (state)
            {
                case State.Patrol:
                    UpdatePatrol();
                    break;

                case State.Chase:
                    UpdateChase();
                    break;

                case State.Flee:
                    UpdateFlee();
                    break;

                case State.Rest:
                    if (Time.time >= restUntil)
                        ReturnToPatrol();
                    break;
            }
        }

        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        bool isMoving =
            AgentReady &&
            !agent.isStopped &&
            agent.velocity.sqrMagnitude > 0.05f;

        if (!string.IsNullOrEmpty(isRunningParam))
            animator.SetBool(isRunningParam, isMoving);

        if (!string.IsNullOrEmpty(spotPlayerParam))
        {
            animator.SetBool(
                spotPlayerParam,
                state == State.Chase
            );
        }
    }

    private void UpdatePatrol()
    {
        if (phone.HasPhone && CanSeePlayer())
        {
            state = State.Chase;
            lastSeenAt = Time.time;

            agent.speed = chaseSpeed;
            agent.isStopped = false;

            if (showDebugMessages)
                Debug.Log("Ladrón: jugador detectado.", this);

            // Permite robar inmediatamente si ya está cerca.
            if (IsWithinInteractionRange(stealDistance) &&
                HasClearView())
            {
                StealPhone();
                return;
            }

            MoveToPlayer();
            return;
        }

        if (!agent.pathPending &&
            (!agent.hasPath || agent.remainingDistance <= 0.35f))
        {
            GoToNextPatrolPoint();
        }
    }

    private void UpdateChase()
    {
        // Otro enemigo pudo haber tomado el celular.
        if (!phone.HasPhone)
        {
            ReturnToPatrol();
            return;
        }

        if (CanSeePlayer())
        {
            lastSeenAt = Time.time;
            MoveToPlayer();
        }
        else if (Time.time - lastSeenAt >= loseSightDelay)
        {
            ReturnToPatrol();
        }

        // Sin visión, sigue hacia su último destino conocido.
    }

    private bool IsWithinInteractionRange(float range)
    {
        if (player == null)
            return false;

        Vector3 difference =
            player.position - transform.position;

        if (Mathf.Abs(difference.y) > maxInteractionHeight)
            return false;

        difference.y = 0f;

        return difference.sqrMagnitude <= range * range;
    }

    private bool CanSeePlayer()
    {
        Vector3 toPlayer = SightTarget - eyes.position;

        if (toPlayer.sqrMagnitude > visionRange * visionRange)
            return false;

        Vector3 horizontal =
            Vector3.ProjectOnPlane(toPlayer, Vector3.up);

        if (horizontal.sqrMagnitude > 0.001f &&
            Vector3.Angle(transform.forward, horizontal)
                > visionAngle * 0.5f)
        {
            return false;
        }

        return HasClearView();
    }

    private bool HasClearView()
    {
        return !Physics.Linecast(
            eyes.position,
            SightTarget,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    private void MoveToPlayer()
    {
        // El filtro usa el mismo tipo de agente y sus áreas.
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (NavMesh.SamplePosition(
                player.position,
                out NavMeshHit hit,
                2f,
                filter))
        {
            SetReachableDestination(hit.position);
        }
    }

    private void StealPhone()
    {
        if (!phone.TrySteal(this))
        {
            if (showDebugMessages)
            {
                Debug.LogWarning(
                    "Ladrón: PhoneState rechazó el robo. " +
                    "Revisa la posesión y la referencia de Phone.",
                    this
                );
            }

            ReturnToPatrol();
            return;
        }

        state = State.Flee;

        agent.ResetPath();
        agent.speed = fleeSpeed;
        agent.isStopped = false;

        if (showDebugMessages)
            Debug.Log("Ladrón: celular robado. Inicia huida.", this);

        if (audioSource != null && screamerClip != null)
            audioSource.PlayOneShot(screamerClip);

        UpdateFlee();
    }

    private void UpdateFlee()
    {
        if (phone.CurrentThief != this)
        {
            ReturnToPatrol();
            return;
        }

        if (agent.hasPath &&
            agent.pathStatus == NavMeshPathStatus.PathComplete &&
            agent.remainingDistance > 1f)
        {
            return;
        }

        Vector3 away = transform.position - player.position;
        away.y = 0f;

        if (away.sqrMagnitude < 0.01f)
            away = transform.forward;

        away.Normalize();

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        float bestScore = float.NegativeInfinity;
        Vector3 bestPosition = transform.position;
        bool found = false;

        for (int i = 0; i < 8; i++)
        {
            Vector3 direction =
                Quaternion.Euler(0f, i * 45f, 0f) * away;

            Vector3 candidate =
                transform.position + direction * fleeStep;

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    1.5f,
                    filter))
            {
                continue;
            }

            if ((hit.position - transform.position).sqrMagnitude < 1f)
                continue;

            if (!agent.CalculatePath(hit.position, reusablePath) ||
                reusablePath.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            Vector3 distanceFromPlayer =
                hit.position - player.position;

            distanceFromPlayer.y = 0f;
            float score = distanceFromPlayer.sqrMagnitude;

            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = hit.position;
                found = true;
            }
        }

        if (found)
            SetReachableDestination(bestPosition);

        // Sin salida válida, permanece acorralado y reintenta
        // en el siguiente intervalo. Se puede recuperar el celular.
    }

    private bool SetReachableDestination(Vector3 destination)
    {
        if (!AgentReady)
            return false;

        if (!agent.CalculatePath(destination, reusablePath) ||
            reusablePath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        agent.isStopped = false;
        return agent.SetPath(reusablePath);
    }

    private void GoToNextPatrolPoint()
    {
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform point = patrolPoints[patrolIndex];

            patrolIndex =
                (patrolIndex + 1) % patrolPoints.Length;

            if (point != null &&
                SetReachableDestination(point.position))
            {
                return;
            }
        }
    }

    private void ReturnToPatrol()
    {
        state = State.Patrol;

        agent.ResetPath();
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        GoToNextPatrolPoint();
    }

    public bool CanRecover()
    {
        return isActiveAndEnabled &&
               AgentReady &&
               phone != null &&
               player != null &&
               eyes != null &&
               state == State.Flee &&
               phone.CurrentThief == this &&
               IsWithinInteractionRange(recoverDistance) &&
               HasClearView();
    }

    public void TryRecover()
    {
        // Valida otra vez al pulsar el botón.
        if (!CanRecover() || !phone.TryRecover(this))
            return;

        state = State.Rest;
        restUntil = Time.time + restDuration;

        agent.ResetPath();
        agent.isStopped = true;

        UpdateAnimations();

        if (showDebugMessages)
            Debug.Log("Ladrón: celular recuperado.", this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stealDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, recoverDistance);

        if (eyes != null)
        {
            Vector3 left =
                Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up)
                * transform.forward;

            Vector3 right =
                Quaternion.AngleAxis(visionAngle * 0.5f, Vector3.up)
                * transform.forward;

            Gizmos.color = Color.yellow;

            Gizmos.DrawRay(eyes.position, left * visionRange);
            Gizmos.DrawRay(eyes.position, right * visionRange);
        }

        // Las esferas muestran los radios como ayuda visual.
        // La interacción real usa distancia horizontal
        // y el límite de altura configurado.
    }
}