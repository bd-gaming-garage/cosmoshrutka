using UnityEngine;
using UnityEngine.InputSystem;

public class Pedal : MonoBehaviour
{
    [SerializeField] private RectTransform _rt;
    [SerializeField] private CursorHand cursor;
    [SerializeField] private float maxPressDepth = 100f;
    [SerializeField] private float returnSpeed = 400f;

    [Range(0f, 1f)]
    public float pressAmount = 0f;

    private Vector2 _startAnchoredPos;
    private Vector2 _grabOffset;
    private bool _isGrabbed;

    private void Awake()
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();
        _startAnchoredPos = _rt.anchoredPosition;
    }

    private void Update()
    {
        if (_isGrabbed) return;

        if (_rt.anchoredPosition != _startAnchoredPos)
        {
            _rt.anchoredPosition = Vector2.MoveTowards(
                _rt.anchoredPosition,
                _startAnchoredPos,
                returnSpeed * Time.deltaTime);

            UpdatePressAmount();
        }
        else
        {
            pressAmount = 0f;
        }
    }

    private void LateUpdate()
    {
        if (!_isGrabbed) return;
        if (cursor == null) return;

        Vector2 worldHand = cursor.transform.position;
        var parentRt = _rt.parent as RectTransform;
        if (parentRt == null) return;

        Vector2 localHand = parentRt.InverseTransformPoint(worldHand);
        Vector2 target = _rt.anchoredPosition;

        target.y = Mathf.Clamp(
            localHand.y + _grabOffset.y,
            _startAnchoredPos.y - maxPressDepth,
            _startAnchoredPos.y);

        _rt.anchoredPosition = target;
        UpdatePressAmount();
    }

    private void UpdatePressAmount()
    {
        float depth = _startAnchoredPos.y - _rt.anchoredPosition.y;
        pressAmount = Mathf.Clamp01(depth / maxPressDepth);
    }

    private void OnMouseDown()
    {
        if (cursor == null) cursor = CursorHand.Instance;
        if (cursor == null) return;
        if (!cursor.TryGrab(gameObject)) return;

        _isGrabbed = true;

        var parentRt = _rt.parent as RectTransform;
        if (parentRt == null) return;

        Vector2 screen = Mouse.current.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt, screen, null, out Vector2 localHand);

        _grabOffset = _rt.anchoredPosition - localHand;
    }

    private void OnMouseUp()
    {
        if (cursor == null) return;
        if (cursor.GrabbedObject == gameObject)
            cursor.Ungrab();
    }

    public void ReleaseGrab()
    {
        _isGrabbed = false;
    }
}