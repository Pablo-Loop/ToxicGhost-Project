using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneRecoveryUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PhoneState phone;
    [SerializeField] private Button recoverButton;

    [Header("Aviso de recuperación")]
    [SerializeField] private TMP_Text recoveredText;

    [SerializeField, Min(0.1f)]
    private float recoveredMessageDuration = 2f;

    [Header("Screamer opcional")]
    [SerializeField] private GameObject screamerVisual;

    [SerializeField, Min(0.05f)]
    private float screamerDuration = 0.3f;

    private Coroutine screamerRoutine;
    private Coroutine recoveredRoutine;

    private void Awake()
    {
        SetButtonVisible(false);

        if (screamerVisual != null)
            screamerVisual.SetActive(false);

        if (recoveredText != null)
            recoveredText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SetButtonVisible(false);

        if (recoverButton != null)
            recoverButton.onClick.AddListener(Recover);

        if (phone != null)
        {
            phone.onStolen.AddListener(OnPhoneStolen);
            phone.onRecovered.AddListener(OnPhoneRecovered);
        }
    }

    private void Update()
    {
        bool canRecover = false;

        if (phone != null && !phone.HasPhone)
        {
            GhostThiefAI thief = phone.CurrentThief;

            canRecover =
                thief != null && thief.CanRecover();
        }

        SetButtonVisible(canRecover);
    }

    private void SetButtonVisible(bool visible)
    {
        if (recoverButton != null &&
            recoverButton.gameObject.activeSelf != visible)
        {
            recoverButton.gameObject.SetActive(visible);
        }
    }

    private void Recover()
    {
        if (phone == null || phone.HasPhone)
        {
            SetButtonVisible(false);
            return;
        }

        GhostThiefAI thief = phone.CurrentThief;

        if (thief == null || !thief.CanRecover())
        {
            SetButtonVisible(false);
            return;
        }

        // El mensaje se muestra únicamente si PhoneState
        // confirma la recuperación mediante su evento.
        thief.TryRecover();
    }

    private void OnPhoneStolen()
    {
        // Retira un aviso anterior si vuelven a robar.
        HideRecoveredMessage();

        if (screamerVisual == null)
            return;

        if (screamerRoutine != null)
            StopCoroutine(screamerRoutine);

        screamerRoutine = StartCoroutine(ShowScreamer());
    }

    private void OnPhoneRecovered()
    {
        SetButtonVisible(false);
        HideRecoveredMessage();

        if (recoveredText != null)
        {
            recoveredRoutine =
                StartCoroutine(ShowRecoveredMessage());
        }
    }

    private IEnumerator ShowRecoveredMessage()
    {
        recoveredText.text = "¡Celular recuperado!";
        recoveredText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(
            recoveredMessageDuration
        );

        recoveredText.gameObject.SetActive(false);
        recoveredRoutine = null;
    }

    private void HideRecoveredMessage()
    {
        if (recoveredRoutine != null)
            StopCoroutine(recoveredRoutine);

        recoveredRoutine = null;

        if (recoveredText != null)
            recoveredText.gameObject.SetActive(false);
    }

    private IEnumerator ShowScreamer()
    {
        screamerVisual.SetActive(true);

        yield return new WaitForSeconds(screamerDuration);

        screamerVisual.SetActive(false);
        screamerRoutine = null;
    }

    private void OnDisable()
    {
        if (recoverButton != null)
            recoverButton.onClick.RemoveListener(Recover);

        if (phone != null)
        {
            phone.onStolen.RemoveListener(OnPhoneStolen);
            phone.onRecovered.RemoveListener(OnPhoneRecovered);
        }

        if (screamerRoutine != null)
            StopCoroutine(screamerRoutine);

        screamerRoutine = null;

        HideRecoveredMessage();
        SetButtonVisible(false);

        if (screamerVisual != null)
            screamerVisual.SetActive(false);
    }
}