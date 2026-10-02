using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SteeringWheel : MonoBehaviour, IGrabbable, IPointerDownHandler, IPointerUpHandler
{
    [FormerlySerializedAs("_wheelrt")] [SerializeField]
    private RectTransform wheelrt;

    [SerializeField] private RectTransform _rt;

    [SerializeField] private GameObject cursor;

    [SerializeField] private float _maxAngle = 360f;
    [SerializeField] private float _currentAngle = 0f;
    [SerializeField] private float _steerInput = 0f;

    private bool _isGrabbed = false;
    private Vector2 _grabPoint;
    [SerializeField] private float _grabAngle;
    [SerializeField] private float _startGrabAngle;

    [SerializeField] private float _radius = 3f;

    private Vector2 _lastMousePos;

    [SerializeField] private float spring = 1f;
    [SerializeField] private float damping = 1f;

    private float _lastDelta;
    private Vector2 _lastDeltaPos;

    private float _angularVelocity = 0f;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        if (!_isGrabbed)
        {
            float force = -spring * (_currentAngle / _maxAngle) * Mathf.Deg2Rad;
            _angularVelocity += force * Time.deltaTime;
            _angularVelocity *= Mathf.Exp(-damping * Time.deltaTime);

            _currentAngle += _angularVelocity * Mathf.Rad2Deg * Time.deltaTime;
            _currentAngle = Mathf.Clamp(_currentAngle, -_maxAngle, _maxAngle);

            wheelrt.localRotation = Quaternion.Euler(0, 0, _currentAngle);
            _steerInput = _currentAngle / _maxAngle;
            return;
        }

        var mouse = Mouse.current;
        if (mouse == null)
        {
            cursor.GetComponent<CursorHand>().Ungrab();
            {
                return;
            }
        }

        Vector2 mousePos = mouse.position.ReadValue();
        Vector2 deltaPos = mousePos - _lastMousePos;

        float deltaLenght = deltaPos.magnitude;

        deltaPos = deltaPos.normalized * math.min(deltaLenght, 120);

        Vector2 orbitStart = Quaternion.Euler(0, 0, _grabAngle) * new Vector2(_radius, 0);
        Vector2 handPos = orbitStart + deltaPos;

        float pointerAngle = Mathf.Atan2(handPos.y, handPos.x) * Mathf.Rad2Deg;
        float delta = Mathf.DeltaAngle(_grabAngle, pointerAngle);

        _currentAngle = Mathf.Clamp(_currentAngle + delta, -_maxAngle, _maxAngle);
        _steerInput = _currentAngle / _maxAngle;

        if (math.abs(_currentAngle) != _maxAngle)
            _grabAngle = pointerAngle;
        else
            _grabAngle = _startGrabAngle;

        wheelrt.localRotation = Quaternion.Euler(0, 0, _currentAngle);

        cursor.transform.position = wheelrt.TransformPoint(_grabPoint);

        _lastMousePos = mousePos;
        _lastDelta = delta;
        _lastDeltaPos = deltaPos;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (!cursor.GetComponent<CursorHand>().TryGrab(gameObject))
        {
            return;
        }

        _lastDelta = 0f;
        _lastDeltaPos = Vector2.zero;

        _lastMousePos = e.position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, e.position, e.pressEventCamera, out Vector2 local);

        _grabAngle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        _startGrabAngle = Mathf.Repeat(_grabAngle - _currentAngle, 360f);
        _isGrabbed = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wheelrt, e.position, e.pressEventCamera, out Vector2 wheelLocal);
        _grabPoint = wheelLocal;
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left)
            return;

        var hand = cursor.GetComponent<CursorHand>();

        if (hand.grabbedObject == gameObject)
            hand.Ungrab();
    }

    public void ReleaseGrab()
    {
        if (!_isGrabbed) return;
        _isGrabbed = false;

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        _angularVelocity =
            math.sign(_lastDelta) *
            math.min(math.abs(_lastDelta), 10) / dt / _radius * 3;

        if (Mouse.current != null)
        {
            Mouse.current.WarpCursorPosition(
                (Vector2)wheelrt.TransformPoint(_grabPoint) +
                _lastDeltaPos.normalized * math.min(_lastDelta, 10));
        }
    }
}