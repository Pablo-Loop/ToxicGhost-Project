using UnityEngine;
using UnityEngine.Events;

public class PhoneState : MonoBehaviour
{
    public bool HasPhone { get; private set; } = true;

    public bool IsDelivered { get; private set; }

    public GhostThiefAI CurrentThief { get; private set; }

    [Header("Eventos")]
    public UnityEvent onStolen = new UnityEvent();
    public UnityEvent onRecovered = new UnityEvent();
    public UnityEvent onDelivered = new UnityEvent();

    public bool TrySteal(GhostThiefAI thief)
    {
        if (IsDelivered || !HasPhone || thief == null)
            return false;

        HasPhone = false;
        CurrentThief = thief;

        onStolen.Invoke();
        return true;
    }

    public bool TryRecover(GhostThiefAI thief)
    {
        if (IsDelivered ||
            HasPhone ||
            thief == null ||
            CurrentThief != thief)
        {
            return false;
        }

        HasPhone = true;
        CurrentThief = null;

        onRecovered.Invoke();
        return true;
    }

    public bool TryDeliver()
    {
        // Solo se puede entregar una vez y estando en poder de Gasp.
        if (IsDelivered || !HasPhone || CurrentThief != null)
            return false;

        IsDelivered = true;
        HasPhone = false;
        CurrentThief = null;

        onDelivered.Invoke();
        return true;
    }
}