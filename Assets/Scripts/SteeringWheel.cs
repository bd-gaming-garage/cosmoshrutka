using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SteeringWheel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] private RectTransform _wheelrt;
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

    void Awake() {
        _rt = GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        if (!_isGrabbed) {
            float force = -spring * (_currentAngle / _maxAngle) * Mathf.Deg2Rad;
            _angularVelocity += force * Time.deltaTime;
            _angularVelocity *= Mathf.Exp(-damping * Time.deltaTime);

            _currentAngle += _angularVelocity * Mathf.Rad2Deg * Time.deltaTime;
            _currentAngle = Mathf.Clamp(_currentAngle, -_maxAngle, _maxAngle);

            _wheelrt.localRotation = Quaternion.Euler(0, 0, _currentAngle);
            _steerInput = _currentAngle / _maxAngle;
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 deltaPos = mousePos - _lastMousePos;

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

        _wheelrt.localRotation = Quaternion.Euler(0, 0, _currentAngle);

        cursor.GetComponent<CursorHand>().Grab(gameObject);
        cursor.transform.position = _wheelrt.TransformPoint(_grabPoint);

        _lastMousePos = mousePos;
        _lastDelta = delta;
        _lastDeltaPos = deltaPos;
    }

    public void OnPointerDown(PointerEventData e){
        _lastMousePos = e.position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, e.position, e.pressEventCamera, out Vector2 local);

        _grabAngle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        _startGrabAngle = Mathf.Repeat(_grabAngle - _currentAngle, 360f);
        _isGrabbed = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _wheelrt, e.position, e.pressEventCamera, out Vector2 wheelLocal);
        _grabPoint = wheelLocal;
    }

    public void OnPointerUp(PointerEventData e) {
        _angularVelocity = _lastDelta / Time.deltaTime / _radius * 3;
        Mouse.current.WarpCursorPosition((Vector2) _wheelrt.TransformPoint(_grabPoint) + _lastDeltaPos.normalized * math.min(_lastDelta, 10));

        _isGrabbed = false;
        cursor.GetComponent<CursorHand>().Ungrab();
    }
}
