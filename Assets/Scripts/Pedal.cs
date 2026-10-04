using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Pedal : GrabbableBehaviour
{
    [SerializeField] private RectTransform rt;
    [SerializeField] private CursorHand cursor;
    [SerializeField, Min(1f)] private float maxPressDepth = 75f;
    [SerializeField, Min(1f)] private float returnSpeed = 300f;

    public float PressAmount => rt == null
        ? 0f
        : Mathf.Clamp01(
            (_startAnchoredPos.y - rt.anchoredPosition.y) /
            Mathf.Max(maxPressDepth, 1f));

    private RectTransform _parentRt;
    private Camera _eventCamera;
    private Vector2 _startAnchoredPos;
    private float _grabOffsetY;

    protected override CursorHand ResolveHand() =>
        cursor != null ? cursor : base.ResolveHand();

    private void Awake()
    {
        if (rt == null) rt = GetComponent<RectTransform>();

        _parentRt = rt.parent as RectTransform;
        _startAnchoredPos = rt.anchoredPosition;
    }

    protected override bool TryPrepareGrab(PointerEventData eventData)
    {
        if (_parentRt == null ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRt, eventData.position, eventData.pressEventCamera,
                out Vector2 pointer))
        {
            return false;
        }

        _eventCamera = eventData.pressEventCamera;
        _grabOffsetY = rt.anchoredPosition.y - pointer.y;
        return true;
    }

    protected override void Update()
    {
        base.Update();

        if (IsGrabbed)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRt, Mouse.current.position.ReadValue(),
                    _eventCamera, out Vector2 pointer))
            {
                float y = Mathf.Clamp(
                    pointer.y + _grabOffsetY,
                    _startAnchoredPos.y - Mathf.Max(maxPressDepth, 1f),
                    _startAnchoredPos.y);

                rt.anchoredPosition = new Vector2(_startAnchoredPos.x, y);
            }
        }
        else
        {
            rt.anchoredPosition = Vector2.MoveTowards(
                rt.anchoredPosition, _startAnchoredPos,
                Mathf.Max(returnSpeed, 1f) * Time.deltaTime);
        }
    }

    public void ResetPress()
    {
        CancelGrab();
    }

    protected override void OnGrabCancelled()
    {
        if (rt != null)
            rt.anchoredPosition = _startAnchoredPos;
    }
}