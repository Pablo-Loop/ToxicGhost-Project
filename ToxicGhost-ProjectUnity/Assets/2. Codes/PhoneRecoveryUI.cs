using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhoneRecoveryUI : MonoBehaviour
{
    [SerializeField] private PhoneState phone;
    [SerializeField] private Button recoverButton;

    [Header("Screamer visual opcional")]
    [SerializeField] private GameObject screamerVisual;
    [SerializeField, Min(0.05f)] private float screamerDuration = 0.3f;

    private Coroutine screamerRoutine;

    private void Awake()
    {
        if (recoverButton != null)
            recoverButton.gameObject.SetActive(false);

        if (screamerVisual != null)
            screamerVisual.SetActive(false);
    }

    private void OnEnable()
    {
        if (recoverButton != null)
            recoverButton.onClick.AddListener(Recover);

        if (phone != null)
            phone.onStolen.AddListener(ShowScreamer);
    }

    private void OnDisable()
    {
        if (recoverButton != null)
        {
            recoverButton.onClick.RemoveListener(Recover);
            recoverButton.gameObject.SetActive(false);
        }

        if (phone != null)
            phone.onStolen.RemoveListener(ShowScreamer);

        if (screamerRoutine != null)
            StopCoroutine(screamerRoutine);

        screamerRoutine = null;

        if (screamerVisual != null)
            screamerVisual.SetActive(false);
    }

    private void Update()
    {
        if (recoverButton == null)
            return;

        GhostThiefAI thief =
            phone != null ? phone.CurrentThief : null;

        bool show = thief != null && thief.CanRecover();

        if (recoverButton.gameObject.activeSelf != show)
            recoverButton.gameObject.SetActive(show);
    }

    private void Recover()
    {
        if (phone != null && phone.CurrentThief != null)
            phone.CurrentThief.TryRecover();
    }

    private void ShowScreamer()
    {
        if (screamerVisual == null)
            return;

        if (screamerRoutine != null)
            StopCoroutine(screamerRoutine);

        screamerRoutine = StartCoroutine(Screamer());
    }

    private IEnumerator Screamer()
    {
        screamerVisual.SetActive(true);

        yield return new WaitForSeconds(screamerDuration);

        screamerVisual.SetActive(false);
        screamerRoutine = null;
    }
}