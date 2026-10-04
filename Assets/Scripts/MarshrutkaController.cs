using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MarshrutkaController : MonoBehaviour
{
    [Header("Engine")]
    [SerializeField, Min(0f)] private float acceleration = 5f;
    [SerializeField, Min(0f)] private float maxForwardSpeed = 15f;
    [SerializeField, Min(0f)] private float maxReverseSpeed = 5f;
        
    [Header("Steering")]
    [SerializeField, Range(0f, 60f)] private float maxSteerAngle = 30f;
    [Tooltip("Wheel steering speed in degrees per second")]
    [SerializeField, Min(0f)] private float steerSpeed = 90f;
    [SerializeField, Min(0.1f)] private float wheelbase = 3f;

    [Header("Braking")]
    [SerializeField, Min(0f)] private float brakeDeceleration = 10f;
    [SerializeField, Min(0f)] private float rollingResistance = 0.3f;
        
    [Header("Traction")] 
    [SerializeField, Min(0f)] private float lateralGrip = 8f;
        
    [Header("Transmission")]
    [Tooltip("Maximum planar speed at which a gear change is allowed")] 
    [SerializeField, Min(0f)] private float maxGearChangeSpeed = 0.01f;

    private Rigidbody _body;
    private MarshrutkaInput _currentInput;
    private float _currentSteerAngle;

    public MarshrutkaGear CurrentGear { get; private set; } = MarshrutkaGear.Drive;

    public bool CanChangeGear => isActiveAndEnabled && _body != null && 
                                 GetPlanarVelocity().sqrMagnitude <= maxGearChangeSpeed * maxGearChangeSpeed;
    
    public float SignedSpeed => _body == null ? 0f : Vector3.Dot(_body.linearVelocity, Forward);

    private Vector3 Forward => _body.rotation * Vector3.forward;
    private Vector3 Right => _body.rotation * Vector3.right;

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _body.useGravity = false;
        _body.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationZ;
    }

    // Returns whether the requested gear was accepted.
    public bool SetInput(MarshrutkaInput input)
    {
        bool validGear = input.Gear == MarshrutkaGear.Drive || input.Gear == MarshrutkaGear.Reverse;
        bool gearAccepted = validGear && (input.Gear == CurrentGear || CanChangeGear);

        if (gearAccepted)
        {
            CurrentGear = input.Gear;
        }

        _currentInput = new MarshrutkaInput(Mathf.Clamp(input.Steering, -1f, 1f),
            Mathf.Clamp01(input.Throttle), Mathf.Clamp01(input.Brake), CurrentGear);

        return gearAccepted;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        _currentSteerAngle = Mathf.MoveTowards(
            _currentSteerAngle, _currentInput.Steering * maxSteerAngle, steerSpeed * dt);

        ApplyResistance(dt);
        ApplyMotor(dt);
        ApplySteering();
    }

    private Vector3 GetPlanarVelocity()
    {
        if (_body == null)
        {
            return Vector3.zero;
        }

        Vector3 velocity = _body.linearVelocity;
        velocity.y = 0f;
        return velocity;
    }

    private void ApplyResistance(float dt)
    {
        Vector3 velocity = GetPlanarVelocity();

        // Reduce lateral sliding without removing it instantly.
        float lateralSpeed = Vector3.Dot(velocity, Right);
        float gripFactor = 1f - Mathf.Exp(-lateralGrip * dt);
        velocity -= Right * (lateralSpeed * gripFactor);

        // Slow down to zero without reversing the velocity.
        float deceleration = rollingResistance + brakeDeceleration * _currentInput.Brake;

        velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * dt);

        _body.linearVelocity = velocity;
    }

    private void ApplyMotor(float dt)
    {
        // Braking takes priority over engine acceleration.
        if (_currentInput.Brake > 0f || _currentInput.Throttle <= 0f)
        {
            return;
        }

        float direction = CurrentGear == MarshrutkaGear.Drive ? 1f : -1f;
        float speedLimit = CurrentGear == MarshrutkaGear.Drive ? maxForwardSpeed : maxReverseSpeed;

        float speedInGearDirection = SignedSpeed * direction;
        float availableAcceleration = Mathf.Max(0f, speedLimit - speedInGearDirection) / dt;

        float motorAcceleration = Mathf.Min(acceleration * _currentInput.Throttle, availableAcceleration);

        _body.AddForce(Forward * (direction * motorAcceleration), ForceMode.Acceleration);
    }

    private void ApplySteering()
    {
        // Signed speed reverses the yaw direction when reversing.
        float turnRate = SignedSpeed / Mathf.Max(wheelbase, 0.1f) *
                            Mathf.Tan(_currentSteerAngle * Mathf.Deg2Rad);

        _body.angularVelocity = Vector3.up * turnRate;
    }

    private void OnDisable()
    {
        _currentInput = new MarshrutkaInput(0f, 0f, 0f, CurrentGear);
    }
}