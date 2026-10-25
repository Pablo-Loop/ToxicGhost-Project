using UnityEngine;
using UnityEngine.EventSystems;

public class CameraLookArea : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler,
    IInitializePotentialDragHandler
{
    [SerializeField] private CrashCameraFollow cameraController;

    private int? activePointerId;

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        // Evita esperar al umbral de arrastre de la UI.
        eventData.useDragThreshold = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!activePointerId.HasValue)
            activePointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!activePointerId.HasValue ||
            eventData.pointerId != activePointerId.Value)
        {
            return;
        }

        if (cameraController != null)
            cameraController.AddLookDelta(eventData.delta);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activePointerId == eventData.pointerId)
            activePointerId = null;
    }

    private void OnDisable()
    {
        activePointerId = null;
    }
}