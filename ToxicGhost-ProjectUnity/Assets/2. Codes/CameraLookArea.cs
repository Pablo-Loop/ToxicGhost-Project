using UnityEngine;
using UnityEngine.EventSystems;

public class CameraLookArea : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler,
    IInitializePotentialDragHandler
{
    [SerializeField]
    private CrashCameraFollow cameraController;

    private int? activePointerId;

    public void OnInitializePotentialDrag(
        PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!activePointerId.HasValue)
            activePointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Solo controla la cámara el dedo que inició este gesto.
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