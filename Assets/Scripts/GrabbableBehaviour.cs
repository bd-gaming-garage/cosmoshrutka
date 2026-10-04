using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public abstract class GrabbableBehaviour : MonoBehaviour,
    IGrabbable, IGrabTarget, IPointerUpHandler
{
    protected bool IsGrabbed { get; private set; }
    protected CursorHand GrabHand { get; private set; }

    protected virtual bool CanContinueGrab => true;

    protected virtual CursorHand ResolveHand() => CursorHand.Instance;

    protected virtual bool TryPrepareGrab(PointerEventData eventData) => true;

    protected virtual void OnGrabStarted(PointerEventData eventData) { }

    protected virtual void OnGrabEnded(GrabEndReason reason) { }

    // Also called when cancellation occurs while the object is not grabbed.
    protected virtual void OnGrabCancelled() { }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || IsGrabbed ||
            !Application.isFocused || !CanContinueGrab ||
            eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        var hand = ResolveHand();
        if (hand == null || !hand.isActiveAndEnabled || hand.IsGrabbing)
            return;

        if (!TryPrepareGrab(eventData) || !hand.TryGrab(gameObject))
            return;

        GrabHand = hand;
        IsGrabbed = true;
        OnGrabStarted(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            EndGrab(GrabEndReason.Released);
    }

    protected virtual void Update()
    {
        if (!IsGrabbed) return;

        var mouse = Mouse.current;
        if (!Application.isFocused || !CanContinueGrab ||
            GrabHand == null || !GrabHand.isActiveAndEnabled ||
            GrabHand.GrabbedObject != gameObject || mouse == null)
        {
            CancelGrab();
            return;
        }

        if (!mouse.leftButton.isPressed)
        {
            EndGrab(mouse.leftButton.wasReleasedThisFrame
                ? GrabEndReason.Released
                : GrabEndReason.Cancelled);
        }
    }

    private void EndGrab(GrabEndReason reason)
    {
        if (!IsGrabbed) return;

        if (GrabHand != null && GrabHand.GrabbedObject == gameObject)
            GrabHand.Ungrab(reason);
        else
            CompleteGrab(GrabEndReason.Cancelled);
    }

    void IGrabbable.ReleaseGrab(GrabEndReason reason)
    {
        CompleteGrab(reason);
    }

    private void CompleteGrab(GrabEndReason reason)
    {
        if (!IsGrabbed) return;

        // Validate every completion path, including release by CursorHand.
        if (!isActiveAndEnabled || !Application.isFocused || !CanContinueGrab ||
            GrabHand == null || !GrabHand.isActiveAndEnabled)
        {
            reason = GrabEndReason.Cancelled;
        }

        IsGrabbed = false;
        GrabHand = null;
        OnGrabEnded(reason);

        if (reason == GrabEndReason.Cancelled)
            OnGrabCancelled();
    }

    protected void CancelGrab()
    {
        if (IsGrabbed)
            EndGrab(GrabEndReason.Cancelled);
        else
            OnGrabCancelled();
    }

    protected virtual void OnDisable()
    {
        CancelGrab();
    }

    protected virtual void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) CancelGrab();
    }
}
