using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class Pedal : MonoBehaviour, IGrabbable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform rt;
    [SerializeField] private CursorHand cursor;
    [SerializeField] [Min(1f)] private float maxPressDepth = 75f;
    [SerializeField] [Min(1f)] private float returnSpeed = 300f;

    public float PressAmount => rt == null
        ? 0f
        : Mathf.Clamp01((_startAnchoredPos.y - rt.anchoredPosition.y) / Mathf.Max(maxPressDepth, 1f));

    private RectTransform _parentRt;
    private Camera _eventCamera;
    private Vector2 _startAnchoredPos;
    private float _grabOffsetY;
    private bool _isGrabbed;

    private void Awake()
    {
        if (rt == null)
        {
            rt = GetComponent<RectTransform>();
        }

        _parentRt = rt.parent as RectTransform;
        _startAnchoredPos = rt.anchoredPosition;
    }

    private void Update()
    {
        if (_isGrabbed)
        {
            var mouse = Mouse.current;
            if (cursor == null || cursor.GrabbedObject != gameObject ||
                mouse == null || !mouse.leftButton.isPressed)
            {
                EndGrab();
            }
            else if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                         _parentRt, mouse.position.ReadValue(),
                         _eventCamera, out Vector2 pointer))
            {
                float y = Mathf.Clamp(
                    pointer.y + _grabOffsetY,
                    _startAnchoredPos.y - Mathf.Max(maxPressDepth, 1f),
                    _startAnchoredPos.y);

                rt.anchoredPosition = new Vector2(_startAnchoredPos.x, y);
            }
        }

        if (!_isGrabbed)
        {
            rt.anchoredPosition = Vector2.MoveTowards(
                rt.anchoredPosition,
                _startAnchoredPos,
                Mathf.Max(returnSpeed, 1f) * Time.deltaTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled ||
            eventData.button != PointerEventData.InputButton.Left ||
            _parentRt == null)
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
        _grabOffsetY = rt.anchoredPosition.y - pointer.y;
        _isGrabbed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            EndGrab();
        }
    }

    public void ReleaseGrab()
    {
        _isGrabbed = false;
    }

    private void EndGrab()
    {
        if (cursor != null && cursor.GrabbedObject == gameObject)
        {
            cursor.Ungrab();
        }

        ReleaseGrab();
    }

    public void ResetPress()
    {
        EndGrab();

        if (rt != null)
        {
            rt.anchoredPosition = _startAnchoredPos;
        }
    }

    private void OnDisable()
    {
        ResetPress();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ResetPress();
        }
    }
}