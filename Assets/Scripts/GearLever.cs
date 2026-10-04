using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class GearLever : MonoBehaviour, IGrabbable, IPointerDownHandler, IPointerUpHandler
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
    private bool _isGrabbed;

    // 0 = Drive, 1 = Reverse.
    private float _position;

    private bool HasControls => controls != null && controls.HasMarshrutka;

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

    private void Update()
    {
        if (_isGrabbed)
        {
            var mouse = Mouse.current;

            if (!HasControls || cursor == null ||
                cursor.GrabbedObject != gameObject || mouse == null)
            {
                CancelGrab();
            }
            else if (!mouse.leftButton.isPressed)
            {
                EndGrab();
            }
            else
            {
                UpdateDrag(mouse.position.ReadValue());
            }
        }

        if (!_isGrabbed)
        {
            _position = Mathf.MoveTowards(
                _position, GearPosition,
                Mathf.Max(snapSpeed, 0.1f) * Time.deltaTime);
            ApplyPosition();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || !HasControls || _parentRt == null ||
            eventData.button != PointerEventData.InputButton.Left ||
            (reversePosition - drivePosition).sqrMagnitude < 0.001f)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRt, eventData.position, eventData.pressEventCamera,
                out Vector2 pointer))
        {
            return;
        }

        if (cursor == null) cursor = CursorHand.Instance;
        if (cursor == null || !cursor.TryGrab(gameObject)) return;

        _eventCamera = eventData.pressEventCamera;
        _grabOffset = _rt.anchoredPosition - pointer;
        _isGrabbed = true;
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

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            EndGrab();
        }
    }

    private void EndGrab()
    {
        if (!_isGrabbed)
        {
            return;
        }
        
        if (cursor != null && cursor.GrabbedObject == gameObject)
        {
            cursor.Ungrab();
        }
        else
        {
            CancelGrab();
        }
    }

    public void ReleaseGrab()
    {
        if (!_isGrabbed) return;
        _isGrabbed = false;

        var mouse = Mouse.current;
        if (!isActiveAndEnabled || !Application.isFocused ||
            !HasControls || mouse == null ||
            !mouse.leftButton.wasReleasedThisFrame)
        {
            SnapToCurrentGear();
            return;
        }

        // Sample the final pointer position and recheck the movement lock.
        UpdateDrag(mouse.position.ReadValue());

        // Releasing at the midpoint preserves the current gear.
        MarshrutkaGear requestedGear = controls.CurrentGear;
        if (_position < 0.5f)
            requestedGear = MarshrutkaGear.Drive;
        else if (_position > 0.5f)
            requestedGear = MarshrutkaGear.Reverse;

        // The controller validates the request again.
        // Update then animates towards the actual accepted gear.
        controls.TrySelectGear(requestedGear);
    }

    private void CancelGrab()
    {
        _isGrabbed = false;

        if (cursor != null && cursor.GrabbedObject == gameObject)
        {
            cursor.Ungrab();
        }

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

    private void OnDisable()
    {
        CancelGrab();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) CancelGrab();
    }
}