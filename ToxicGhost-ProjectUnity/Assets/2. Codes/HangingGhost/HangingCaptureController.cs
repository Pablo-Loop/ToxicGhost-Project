using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HangingCaptureController : MonoBehaviour
{
    private enum CaptureState
    {
        Idle,
        Playing,
        Resolving
    }

    [Header("Jugador")]
    [SerializeField] private Rigidbody playerBody;

    [Header("Controles del jugador")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CrashCameraFollow cameraFollow;
    [SerializeField] private CameraLookArea cameraLookArea;

    [Tooltip("Otros scripts que permiten abrir inventario, teléfono o habilidades.")]
    [SerializeField] private MonoBehaviour[] additionalControls;
    [SerializeField] private LivesSystem livesSystem;

    [Header("Interfaz habitual")]
    [Tooltip("No incluir la salud, el panel QTE ni sus objetos padres.")]
    [SerializeField] private GameObject[] hudToHide;

    [Header("Interfaz de captura")]
    [SerializeField] private GameObject capturePanel;
    [SerializeField] private RectTransform tapArea;
    [SerializeField] private Button escapeButton;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Slider timeSlider;

    [Header("Dificultad")]
    [SerializeField, Min(0.1f)]
    private float timeLimit = 6f;

    [SerializeField, Range(0.01f, 1f)]
    private float progressPerTap = 0.1f;

    [SerializeField, Min(0f)]
    private float progressDecayPerSecond = 0.08f;

    [SerializeField, Min(0f)]
    private float failureDamage = 1f; // Daño configurado por defecto a 1 vida

    [Tooltip("Tiempo reservado para la animación de resolución.")]
    [SerializeField, Min(0f)]
    private float resolutionDuration = 0.6f;

    [Header("Animaciones y efectos")]
    public UnityEvent onCaptured = new UnityEvent();
    public UnityEvent onEscaped = new UnityEvent();
    public UnityEvent onFailed = new UnityEvent();

    [Header("Conexión con la salud")]
    public UnityEvent<float> onFailureDamage = new UnityEvent<float>();

    private CaptureState state = CaptureState.Idle;
    private Component owner;

    private Behaviour[] controlsToDisable;
    private bool[] previousControlStates;
    private bool[] previousHUDStates;

    private RigidbodyConstraints previousConstraints;

    private float progress;
    private float remainingTime;
    private float resolutionRemaining;
    private bool escaped;

    public bool IsBusy => state != CaptureState.Idle;

    public bool IsOwnedBy(Component requester)
    {
        return IsBusy && owner == requester;
    }

    private void Awake()
    {
        BuildControlList();

        if (capturePanel != null)
            capturePanel.SetActive(false);
    }

    private void BuildControlList()
    {
        var controls = new System.Collections.Generic.List<Behaviour>();

        AddControl(controls, playerController);
        AddControl(controls, cameraFollow);
        AddControl(controls, cameraLookArea);

        if (additionalControls != null)
        {
            foreach (MonoBehaviour control in additionalControls)
                AddControl(controls, control);
        }

        controlsToDisable = controls.ToArray();
    }

    private void AddControl(
        System.Collections.Generic.List<Behaviour> controls,
        Behaviour control)
    {
        if (control == null || control == this ||
            controls.Contains(control))
        {
            return;
        }

        controls.Add(control);
    }

    private void OnEnable()
    {
        if (escapeButton != null)
            escapeButton.onClick.AddListener(RegisterTap);
    }

    private void OnDisable()
    {
        if (escapeButton != null)
            escapeButton.onClick.RemoveListener(RegisterTap);

        if (IsBusy)
            RestoreState();
    }

    public bool TryCapture(Component requester)
    {
        if (!isActiveAndEnabled || IsBusy || requester == null)
            return false;

        if (!ValidateReferences())
            return false;

        if (playerBody.isKinematic ||
            playerBody.constraints == RigidbodyConstraints.FreezeAll ||
            !playerController.isActiveAndEnabled)
        {
            return false;
        }

        BuildControlList();

        owner = requester;
        state = CaptureState.Playing;

        SaveAndLockState();

        progress = 0f;
        remainingTime = timeLimit;

        capturePanel.SetActive(true);

        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;

        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = 1f;
        }

        Canvas.ForceUpdateCanvases();

        PlaceButton();
        RefreshBars();

        onCaptured.Invoke();
        return true;
    }

    private bool ValidateReferences()
    {
        if (playerBody == null ||
            playerController == null ||
            cameraFollow == null ||
            cameraLookArea == null ||
            capturePanel == null ||
            tapArea == null ||
            escapeButton == null ||
            progressSlider == null)
        {
            Debug.LogError(
                "HangingCaptureController: faltan referencias. " +
                "Revisa jugador, controles y elementos del QTE.",
                this
            );

            return false;
        }

        if (transform.IsChildOf(capturePanel.transform))
        {
            Debug.LogError(
                "HangingCaptureSystem debe estar fuera de CapturePanel.",
                this
            );

            return false;
        }

        if (escapeButton.transform.parent != tapArea)
        {
            Debug.LogError(
                "EscapeButton debe ser hijo directo de TapArea.",
                this
            );

            return false;
        }

        if (hudToHide != null)
        {
            foreach (GameObject hud in hudToHide)
            {
                if (hud == null)
                    continue;

                if (transform.IsChildOf(hud.transform) ||
                    playerBody.transform.IsChildOf(hud.transform) ||
                    cameraFollow.transform.IsChildOf(hud.transform) ||
                    capturePanel.transform.IsChildOf(hud.transform))
                {
                    Debug.LogError(
                        "HUD To Hide contiene un objeto que no debe " +
                        "ocultarse: " + hud.name +
                        ". Incluye únicamente la interfaz habitual.",
                        this
                    );

                    return false;
                }
            }
        }

        return true;
    }

    private void SaveAndLockState()
    {
        previousControlStates = new bool[controlsToDisable.Length];

        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            Behaviour control = controlsToDisable[i];
            previousControlStates[i] = control.enabled;
            control.enabled = false;
        }

        previousConstraints = playerBody.constraints;

        playerBody.linearVelocity = Vector3.zero;
        playerBody.angularVelocity = Vector3.zero;

        playerBody.constraints = RigidbodyConstraints.FreezeAll;

        int hudCount = hudToHide != null ? hudToHide.Length : 0;
        previousHUDStates = new bool[hudCount];

        for (int i = 0; i < hudCount; i++)
        {
            if (hudToHide[i] != null)
                previousHUDStates[i] = hudToHide[i].activeSelf;
        }

        for (int i = 0; i < hudCount; i++)
        {
            if (hudToHide[i] != null)
                hudToHide[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (state == CaptureState.Playing)
        {
            remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);

            progress = Mathf.Max(
                0f,
                progress - progressDecayPerSecond * Time.deltaTime
            );

            RefreshBars();

            if (remainingTime <= 0f)
                BeginResolution(false);
        }
        else if (state == CaptureState.Resolving)
        {
            resolutionRemaining -= Time.deltaTime;

            if (resolutionRemaining <= 0f)
                FinishResolution();
        }
    }

    private void RegisterTap()
    {
        if (state != CaptureState.Playing ||
            Time.timeScale <= 0f ||
            remainingTime <= 0f)
        {
            return;
        }

        progress = Mathf.Clamp01(progress + progressPerTap);
        RefreshBars();

        if (progress >= 1f)
            BeginResolution(true);
    }

    private void BeginResolution(bool success)
    {
        if (state != CaptureState.Playing)
            return;

        escaped = success;
        state = CaptureState.Resolving;
        resolutionRemaining = resolutionDuration;

        capturePanel.SetActive(false);

        if (success)
        {
            // 1. Reproducir sonido de muerte del fantasma
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ReproducirMuerteFantasma();
            }

            // 2. Destruir el GameObject del fantasma que atrapó al jugador
            if (owner != null)
            {
                Destroy(owner.gameObject);
            }

            onEscaped.Invoke();
        }
        else
        {
            onFailed.Invoke();
        }
    }

    private void FinishResolution()
    {
        bool applyDamage = !escaped;

        RestoreState();

        if (applyDamage)
        {
           
            livesSystem.TakeDamage(1);
         
        }
    }

    private void RefreshBars()
    {
        progressSlider.value = progress;

        if (timeSlider != null)
        {
            timeSlider.value = Mathf.Clamp01(remainingTime / timeLimit);
        }
    }

    private void PlaceButton()
    {
        RectTransform buttonRect = (RectTransform)escapeButton.transform;

        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);

        Rect area = tapArea.rect;
        Vector2 halfSize = buttonRect.rect.size * 0.5f;

        float minX = area.xMin + halfSize.x;
        float maxX = area.xMax - halfSize.x;

        float minY = area.yMin + halfSize.y;
        float maxY = area.yMax - halfSize.y;

        Vector2 position = new Vector2(
            minX <= maxX ? Random.Range(minX, maxX) : area.center.x,
            minY <= maxY ? Random.Range(minY, maxY) : area.center.y
        );

        buttonRect.anchoredPosition = position - area.center;
    }

    private void RestoreState()
    {
        if (playerBody != null)
        {
            if (!playerBody.isKinematic)
            {
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }

            playerBody.constraints = previousConstraints;
        }

        if (previousHUDStates != null && hudToHide != null)
        {
            for (int i = 0; i < previousHUDStates.Length && i < hudToHide.Length; i++)
            {
                if (hudToHide[i] != null)
                    hudToHide[i].SetActive(previousHUDStates[i]);
            }
        }

        if (previousControlStates != null && controlsToDisable != null)
        {
            for (int i = 0; i < controlsToDisable.Length; i++)
            {
                Behaviour control = controlsToDisable[i];

                if (control != null)
                    control.enabled = previousControlStates[i];
            }
        }

        if (capturePanel != null)
            capturePanel.SetActive(false);

        owner = null;
        state = CaptureState.Idle;
    }

    public void CancelCapture(Component requester)
    {
        if (IsOwnedBy(requester))
            RestoreState();
    }
}