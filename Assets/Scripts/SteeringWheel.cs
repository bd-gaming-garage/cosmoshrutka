using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SteeringWheel : GrabbableBehaviour
{
    [FormerlySerializedAs("_wheelrt")] [SerializeField]
    private RectTransform wheelrt;

    [SerializeField] private RectTransform _rt;

    [SerializeField] private GameObject cursor;

    [SerializeField] private float _maxAngle = 360f;
    [SerializeField] private float _currentAngle = 0f;

    private Vector2 _grabPoint;
    [SerializeField] private float _grabAngle;
    [SerializeField] private float _startGrabAngle;

    [SerializeField] private float _radius = 3f;

    private Vector2 _lastMousePos;

    [SerializeField] private float spring = 1f;
    [SerializeField] private float damping = 1f;

    [Tooltip("Vehicle speed in m/s at which self-centering reaches full strength")]
    [SerializeField, Min(0.1f)] private float fullReturnSpeed = 10f;

    private Vector2 _lastDeltaPos;

    private float _angularVelocity = 0f;

    protected override CursorHand ResolveHand() =>
        cursor != null
            ? cursor.GetComponent<CursorHand>()
            : base.ResolveHand();

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    protected override void Update()
    {
        base.Update();

        if (!IsGrabbed)
        {
            return;
        }

        var mouse = Mouse.current;
        Vector2 mousePos = mouse.position.ReadValue();
        Vector2 deltaPos = mousePos - _lastMousePos;

        float deltaLenght = deltaPos.magnitude;

        deltaPos = deltaPos.normalized * math.min(deltaLenght, 120);

        Vector2 orbitStart = Quaternion.Euler(0, 0, _grabAngle) * new Vector2(_radius, 0);
        Vector2 handPos = orbitStart + deltaPos;

        float pointerAngle = Mathf.Atan2(handPos.y, handPos.x) * Mathf.Rad2Deg;
        float delta = Mathf.DeltaAngle(_grabAngle, pointerAngle);

        float previousAngle = _currentAngle;
        _currentAngle = Mathf.Clamp(_currentAngle + delta, -_maxAngle, _maxAngle);
        float actualDelta = _currentAngle - previousAngle;

        _angularVelocity = Time.deltaTime > 0f ? actualDelta * Mathf.Deg2Rad / Time.deltaTime : 0f;

        if (math.abs(_currentAngle) != _maxAngle)
        {
            _grabAngle = pointerAngle;
        }
        else
        {
            _grabAngle = _startGrabAngle;
        }

        wheelrt.localRotation = Quaternion.Euler(0, 0, _currentAngle);

        GrabHand.transform.position = wheelrt.TransformPoint(_grabPoint);

        _lastMousePos = mousePos;
        _lastDeltaPos = deltaPos;
    }

    protected override void OnGrabStarted(PointerEventData e)
    {
        _angularVelocity = 0f;
        _lastDeltaPos = Vector2.zero;

        _lastMousePos = e.position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, e.position, e.pressEventCamera, out Vector2 local);

        _grabAngle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        _startGrabAngle = Mathf.Repeat(_grabAngle - _currentAngle, 360f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wheelrt, e.position, e.pressEventCamera, out Vector2 wheelLocal);
        _grabPoint = wheelLocal;
    }

    protected override void OnGrabEnded(GrabEndReason reason)
    {
        if (reason == GrabEndReason.Released && Mouse.current != null)
        {
            Mouse.current.WarpCursorPosition(
                (Vector2)wheelrt.TransformPoint(_grabPoint) +
                Vector2.ClampMagnitude(_lastDeltaPos, 10f));
        }
    }

    protected override void OnGrabCancelled()
    {
        _angularVelocity = 0f;
        _lastDeltaPos = Vector2.zero;
    }

    public void UpdateSelfCentering(float signedSpeed, float deltaTime)
    {
        if (!isActiveAndEnabled || IsGrabbed) return;

        float speed = Mathf.Abs(signedSpeed);
        const float stopSpeed = 0.01f;

        if (_maxAngle <= 0f)
        {
            _angularVelocity = 0f;
            return;
        }

        float speedFactor = Mathf.InverseLerp(
            stopSpeed, Mathf.Max(fullReturnSpeed, 0.1f), speed);
        float dt = Mathf.Max(deltaTime, 0f);

        float force = -spring * speedFactor *
            (_currentAngle / _maxAngle) * Mathf.Deg2Rad;
        _angularVelocity += force * dt;
        _angularVelocity *= Mathf.Exp(-damping * dt);

        _currentAngle = Mathf.Clamp(
            _currentAngle + _angularVelocity * Mathf.Rad2Deg * dt,
            -_maxAngle, _maxAngle);

        wheelrt.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
    }

    public float SteeringInput => _maxAngle > 0f
        ? Mathf.Clamp(-_currentAngle / _maxAngle, -1f, 1f)
        : 0f;
}