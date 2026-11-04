using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MayaUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MayaFleeAI maya;
    [SerializeField] private PhoneState phone;
    [SerializeField] private Button returnPhoneButton;

    [Header("Confirmación")]
    [SerializeField] private TMP_Text confirmationText;

    [SerializeField, Min(0.1f)]
    private float messageDuration = 3f;

    private Coroutine messageRoutine;

    private void Awake()
    {
        SetButtonVisible(false);

        if (confirmationText != null)
            confirmationText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SetButtonVisible(false);

        if (returnPhoneButton != null)
            returnPhoneButton.onClick.AddListener(ReturnPhone);

        if (phone != null)
            phone.onDelivered.AddListener(ShowConfirmation);
    }

    private void Update()
    {
        bool show =
            phone != null &&
            phone.HasPhone &&
            !phone.IsDelivered &&
            maya != null &&
            maya.CanReturnPhone();

        SetButtonVisible(show);
    }

    private void ReturnPhone()
    {
        if (maya == null)
            return;

        if (maya.TryReturnPhone())
            SetButtonVisible(false);
    }

    private void SetButtonVisible(bool visible)
    {
        if (returnPhoneButton != null &&
            returnPhoneButton.gameObject.activeSelf != visible)
        {
            returnPhoneButton.gameObject.SetActive(visible);
        }
    }

    private void ShowConfirmation()
    {
        SetButtonVisible(false);

        if (confirmationText == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(Confirmation());
    }

    private IEnumerator Confirmation()
    {
        confirmationText.text =
            "¡Le devolviste el celular a Maya!";

        confirmationText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(messageDuration);

        confirmationText.gameObject.SetActive(false);
        messageRoutine = null;
    }

    private void OnDisable()
    {
        if (returnPhoneButton != null)
            returnPhoneButton.onClick.RemoveListener(ReturnPhone);

        if (phone != null)
            phone.onDelivered.RemoveListener(ShowConfirmation);

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = null;

        SetButtonVisible(false);

        if (confirmationText != null)
            confirmationText.gameObject.SetActive(false);
    }
}