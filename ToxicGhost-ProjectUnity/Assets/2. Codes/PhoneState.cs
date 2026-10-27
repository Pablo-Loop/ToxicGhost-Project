using UnityEngine;
using UnityEngine.Events;

public class PhoneState : MonoBehaviour
{
    public bool HasPhone { get; private set; } = true;
    public GhostThiefAI CurrentThief { get; private set; }

    [Header("Eventos")]
    public UnityEvent onStolen = new UnityEvent();
    public UnityEvent onRecovered = new UnityEvent();

    public bool TrySteal(GhostThiefAI thief)
    {
        if (!HasPhone || thief == null)
            return false;

        HasPhone = false;
        CurrentThief = thief;

        onStolen.Invoke();
        return true;
    }

    public bool TryRecover(GhostThiefAI thief)
    {
        if (HasPhone || thief == null || CurrentThief != thief)
            return false;

        HasPhone = true;
        CurrentThief = null;

        onRecovered.Invoke();
        return true;
    }
}