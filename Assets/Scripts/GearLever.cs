using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class GearLever : GrabbableBehaviour
{
    [SerializeField] private PlayerMarshrutkaControls controls;
    [SerializeField] private CursorHand cursor;

    [SerializeField] private Vector2 drivePosition = new(-3f, -230f);
    [SerializeField] private Vector2 reversePosition = new(21f, -369f);

    [Tooltip("Travel speed in full segment lengths per second")] [SerializeField, Min(0.1f)]
    private float snapSpeed = 4f;

    private RectTransform _rt;
    private RectTransform _parentRt;
    private Camera _eventCamera;
    private Vector2 _grabOffset;

    // 0 = Drive, 1 = Reverse.
    private float _position;

    private bool HasControls => controls != null && controls.HasMarshrutka;

    protected override bool CanContinueGrab => HasControls;

    protected override CursorHand ResolveHand() =>
        cursor != null ? cursor : base.ResolveHand();

    private float GearPosition =>
        (controls != null && controls.CurrentGear == MarshrutkaGear.Reverse) ? 1f : 0f;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _parentRt = _rt.parent as RectTransform;
    }

    private void OnEnable()
    {
        SnapToCurrentGear();
    }

    protected override void Update()
    {
        base.Update();

        if (IsGrabbed)
        {
            UpdateDrag(Mouse.current.position.ReadValue());
        }
        else
        {
            _position = Mathf.MoveTowards(
                _position, GearPosition,
                Mathf.Max(snapSpeed, 0.1f) * Time.deltaTime);
            ApplyPosition();
        }
    }

    protected override bool TryPrepareGrab(PointerEventData eventData)
    {
        if (_parentRt == null ||
            (reversePosition - drivePosition).sqrMagnitude < 0.001f)
        {
            return false;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRt, eventData.position, eventData.pressEventCamera,
                out Vector2 pointer))
        {
            return false;
        }

        _eventCamera = eventData.pressEventCamera;
        _grabOffset = _rt.anchoredPosition - pointer;
        return true;
    }

    protected override void OnGrabStarted(PointerEventData eventData)
    {
        ClampToAllowedRange();
        ApplyPosition();
    }

    private void UpdateDrag(Vector2 screenPosition)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRt, screenPosition, _eventCamera,
                out Vector2 pointer))
        {
            Vector2 segment = reversePosition - drivePosition;
            Vector2 target = pointer + _grabOffset;

            _position = Mathf.Clamp01(
                Vector2.Dot(target - drivePosition, segment) /
                Mathf.Max(segment.sqrMagnitude, 0.001f));
        }

        ClampToAllowedRange();
        ApplyPosition();
    }

    private void ClampToAllowedRange()
    {
        if (!controls.CanChangeGear)
        {
            _position = controls.CurrentGear == MarshrutkaGear.Drive
                ? Mathf.Min(_position, 0.5f)
                : Mathf.Max(_position, 0.5f);
        }
    }

    protected override void OnGrabEnded(GrabEndReason reason)
    {
        if (reason != GrabEndReason.Released) return;

        var mouse = Mouse.current;
        if (!HasControls || mouse == null)
        {
            SnapToCurrentGear();
            return;
        }

        UpdateDrag(mouse.position.ReadValue());

        MarshrutkaGear requestedGear = controls.CurrentGear;
        if (_position < 0.5f)
            requestedGear = MarshrutkaGear.Drive;
        else if (_position > 0.5f)
            requestedGear = MarshrutkaGear.Reverse;

        controls.TrySelectGear(requestedGear);
    }

    protected override void OnGrabCancelled()
    {
        SnapToCurrentGear();
    }

    private void SnapToCurrentGear()
    {
        _position = GearPosition;
        if (_rt != null) ApplyPosition();
    }

    private void ApplyPosition()
    {
        _rt.anchoredPosition = Vector2.Lerp(
            drivePosition, reversePosition, _position);
    }
}