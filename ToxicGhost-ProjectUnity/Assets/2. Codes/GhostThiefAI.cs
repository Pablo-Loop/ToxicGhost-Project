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

    [Header("Visión")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField, Min(0.1f)] private float visionRange = 9f;
    [SerializeField, Range(1f, 180f)] private float visionAngle = 90f;
    [SerializeField, Min(0f)] private float loseSightDelay = 1.5f;

    [Header("Velocidades")]
    [SerializeField, Min(0f)] private float patrolSpeed = 2f;
    [SerializeField, Min(0f)] private float chaseSpeed = 4f;
    [SerializeField, Min(0f)] private float fleeSpeed = 4.5f;

    [Header("Interacción")]
    [SerializeField, Min(0.1f)] private float stealDistance = 1.4f;
    [SerializeField, Min(0.1f)] private float recoverDistance = 2f;
    [SerializeField, Min(0f)] private float restDuration = 4f;

    [Header("Huida")]
    [SerializeField, Min(1f)] private float fleeStep = 6f;
    [SerializeField, Min(0.1f)] private float thinkInterval = 0.2f;

    [Header("Screamer")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamerClip;

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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        reusablePath = new NavMeshPath();
    }

    private void Start()
    {
        if (player == null || eyes == null || phone == null ||
            patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError(
                "Asigna jugador, ojos, PhoneState y puntos de patrulla.",
                this
            );
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(
                "El Ladrón debe comenzar sobre el NavMesh.",
                this
            );
            enabled = false;
            return;
        }

        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (!agent.isOnNavMesh || Time.time < nextThink)
            return;

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

    private void UpdatePatrol()
    {
        if (phone.HasPhone && CanSeePlayer())
        {
            state = State.Chase;
            lastSeenAt = Time.time;
            agent.speed = chaseSpeed;
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
        // Otro enemigo pudo haber robado el teléfono.
        if (!phone.HasPhone)
        {
            ReturnToPatrol();
            return;
        }

        bool seesPlayer = CanSeePlayer();

        if (seesPlayer)
        {
            lastSeenAt = Time.time;

            if (Vector3.Distance(transform.position, player.position)
                <= stealDistance)
            {
                StealPhone();
                return;
            }

            MoveToPlayer();
        }
        else if (Time.time - lastSeenAt >= loseSightDelay)
        {
            ReturnToPatrol();
        }

        // Sin visión, sigue únicamente hacia la última posición
        // enviada al agente; no conoce la ubicación nueva del jugador.
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
        if (NavMesh.SamplePosition(
                player.position,
                out NavMeshHit hit,
                2f,
                agent.areaMask))
        {
            SetReachableDestination(hit.position);
        }
    }

    private void StealPhone()
    {
        // Se revalida la posesión para evitar robos simultáneos.
        if (!phone.TrySteal(this))
        {
            ReturnToPatrol();
            return;
        }

        state = State.Flee;
        agent.speed = fleeSpeed;
        agent.ResetPath();

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

        // Mantener el destino evita calcular rutas en cada frame.
        if (agent.hasPath && agent.remainingDistance > 1f)
            return;

        Vector3 away = transform.position - player.position;
        away.y = 0f;

        if (away.sqrMagnitude < 0.01f)
            away = transform.forward;

        away.Normalize();

        float bestScore = float.NegativeInfinity;
        Vector3 bestPosition = transform.position;
        bool found = false;

        // Examina varias salidas y acepta solo rutas completas.
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
                    agent.areaMask))
            {
                continue;
            }

            if (Vector3.Distance(transform.position, hit.position) < 1f)
                continue;

            if (!agent.CalculatePath(hit.position, reusablePath) ||
                reusablePath.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            float score =
                (hit.position - player.position).sqrMagnitude;

            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = hit.position;
                found = true;
            }
        }

        if (found)
            SetReachableDestination(bestPosition);
    }

    private bool SetReachableDestination(Vector3 destination)
    {
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
        // Prueba cada punto una vez por llamada.
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform point = patrolPoints[patrolIndex];

            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

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
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        agent.ResetPath();
        GoToNextPatrolPoint();
    }

    public bool CanRecover()
    {
        return isActiveAndEnabled &&
               state == State.Flee &&
               phone.CurrentThief == this &&
               Vector3.Distance(
                   transform.position,
                   player.position
               ) <= recoverDistance &&
               HasClearView();
    }

    public void TryRecover()
    {
        // El botón puede haber quedado visible desde el frame anterior.
        if (!CanRecover() || !phone.TryRecover(this))
            return;

        state = State.Rest;
        restUntil = Time.time + restDuration;

        agent.ResetPath();
        agent.isStopped = true;
    }
}