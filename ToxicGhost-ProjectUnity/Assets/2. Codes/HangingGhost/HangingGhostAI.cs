using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class HangingGhostAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform eyes;
    [SerializeField] private Transform ropeStart;
    [SerializeField] private HangingCaptureController captureController;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private LineRenderer rope;

    [Header("Detección")]
    [Tooltip("Excluir Player y la capa del enemigo.")]
    [SerializeField] private LayerMask obstacleLayers;

    [SerializeField, Min(0.1f)] private float visionRange = 7f;
    [SerializeField, Range(1f, 180f)] private float visionAngle = 90f;

    [Tooltip("Detecta también por detrás a esta distancia. 0 lo desactiva.")]
    [SerializeField, Min(0f)] private float alertRadius = 2f;

    [SerializeField, Min(0.05f)]
    private float detectionInterval = 0.15f;

    [Header("Patrulla y ataque")]
    [SerializeField, Min(0f)] private float patrolSpeed = 2f;
    [SerializeField, Min(0.05f)] private float throwDuration = 0.5f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 5f;

    [Header("Eventos de animación")]
    public UnityEvent onThrow = new UnityEvent();
    public UnityEvent onAttackFinished = new UnityEvent();

    private NavMeshAgent agent;
    private NavMeshPath reusablePath;

    private int patrolIndex;
    private float nextDetection;
    private float nextAttack;

    private bool attacking;
    private Coroutine attackRoutine;

    private bool AgentReady =>
        agent != null &&
        agent.isActiveAndEnabled &&
        agent.isOnNavMesh;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        reusablePath = new NavMeshPath();

        if (rope != null)
        {
            rope.useWorldSpace = true;
            rope.positionCount = 2;
            rope.enabled = false;
        }
    }

    private void Start()
    {
        if (playerTarget == null || eyes == null ||
            captureController == null ||
            patrolPoints == null || patrolPoints.Length == 0 ||
            !AgentReady)
        {
            Debug.LogError(
                "HangingGhost: revisa referencias, patrulla y NavMesh.",
                this
            );

            enabled = false;
            return;
        }

        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;
        agent.isStopped = false;

        GoToNextPoint();
    }

    private void Update()
    {
        if (!AgentReady || attacking || Time.time < nextDetection)
            return;

        nextDetection = Time.time + detectionInterval;

        if (!agent.pathPending &&
            (!agent.hasPath || agent.remainingDistance <= 0.35f))
        {
            GoToNextPoint();
        }

        if (Time.time >= nextAttack &&
            captureController.isActiveAndEnabled &&
            !captureController.IsBusy &&
            CanDetectPlayer())
        {
            attackRoutine = StartCoroutine(Attack());
        }
    }

    private bool CanDetectPlayer()
    {
        Vector3 offset = playerTarget.position - eyes.position;
        float distanceSquared = offset.sqrMagnitude;

        bool inAlertRadius =
            alertRadius > 0f &&
            distanceSquared <= alertRadius * alertRadius;

        bool inVisionCone = false;

        if (distanceSquared <= visionRange * visionRange)
        {
            Vector3 horizontal =
                Vector3.ProjectOnPlane(offset, Vector3.up);

            inVisionCone =
                horizontal.sqrMagnitude < 0.001f ||
                Vector3.Angle(transform.forward, horizontal)
                    <= visionAngle * 0.5f;
        }

        return (inAlertRadius || inVisionCone) && HasClearView();
    }

    private bool HasClearView()
    {
        return !Physics.Linecast(
            eyes.position,
            playerTarget.position,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    private IEnumerator Attack()
    {
        attacking = true;
        agent.ResetPath();
        agent.isStopped = true;

        Vector3 facing = playerTarget.position - transform.position;
        facing.y = 0f;

        if (facing.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(facing);

        onThrow.Invoke();

        if (rope != null)
            rope.enabled = true;

        float elapsed = 0f;

        // Lanzamiento visual, sin física de proyectil.
        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 origin = ropeStart != null
                ? ropeStart.position
                : eyes.position;

            if (rope != null)
            {
                rope.SetPosition(0, origin);

                rope.SetPosition(
                    1,
                    Vector3.Lerp(
                        origin,
                        playerTarget.position,
                        Mathf.Clamp01(elapsed / throwDuration)
                    )
                );
            }

            yield return null;
        }

        float maxAttackRange = Mathf.Max(visionRange, alertRadius);

        bool canImpact =
            (playerTarget.position - eyes.position).sqrMagnitude
                <= maxAttackRange * maxAttackRange &&
            HasClearView();

        // Se puede esquivar durante la preparación,
        // saliendo de alcance o poniéndose detrás de un obstáculo.
        if (canImpact)
            captureController.TryCapture(this);

        while (captureController.IsOwnedBy(this))
        {
            if (rope != null)
            {
                rope.SetPosition(
                    0,
                    ropeStart != null
                        ? ropeStart.position
                        : eyes.position
                );

                rope.SetPosition(1, playerTarget.position);
            }

            yield return null;
        }

        if (rope != null)
            rope.enabled = false;

        nextAttack = Time.time + attackCooldown;
        attacking = false;
        attackRoutine = null;

        if (AgentReady)
        {
            agent.isStopped = false;
            GoToNextPoint();
        }

        onAttackFinished.Invoke();
    }

    private void GoToNextPoint()
    {
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform point = patrolPoints[patrolIndex];

            patrolIndex =
                (patrolIndex + 1) % patrolPoints.Length;

            if (point == null)
                continue;

            if (agent.CalculatePath(point.position, reusablePath) &&
                reusablePath.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetPath(reusablePath);
                return;
            }
        }
    }

    private void OnDisable()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = null;

        if (captureController != null)
            captureController.CancelCapture(this);

        if (rope != null)
            rope.enabled = false;

        if (AgentReady)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        attacking = false;
        nextAttack = Time.time + attackCooldown;
    }

    private void OnEnable()
    {
        if (AgentReady)
            agent.isStopped = false;
    }
}