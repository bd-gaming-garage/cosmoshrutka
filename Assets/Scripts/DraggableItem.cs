using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : GrabbableBehaviour
{
    private Vector2 _localPoint;
    [SerializeField] private CursorHand hand;
    [SerializeField] private int value;
    private RectTransform _rt;

    [SerializeField] private string pickSound = null;
    [SerializeField] private string dropSound = null;

    [Header("Banknote parent on grab")] [SerializeField]
    private string banknotesParentTag = "BanknotesParent";

    [Header("Inertia")] [Tooltip("Higher friction slows the item down faster")] [SerializeField]
    private float friction = 4f;

    [Tooltip("Stop below this speed, in world units per second")] [SerializeField]
    private float minVelocity = 20f;

    [Tooltip("Maximum throw speed in world units per second")] [SerializeField]
    private float maxVelocity = 3000f;

    private Vector3 _lastWorldPos;
    private Vector3 _velocity;

    protected override CursorHand ResolveHand() =>
        hand != null ? hand : base.ResolveHand();

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        _lastWorldPos = transform.position;
    }

    private void LateUpdate()
    {
        if (IsGrabbed)
        {
            transform.position = GrabHand.transform.position - (Vector3)_localPoint;

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            _velocity = (transform.position - _lastWorldPos) / dt;

            if (_velocity.magnitude > maxVelocity)
                _velocity = _velocity.normalized * maxVelocity;

            _lastWorldPos = transform.position;
            return;
        }

        if (_velocity.sqrMagnitude > minVelocity * minVelocity)
        {
            transform.position += _velocity * Time.deltaTime;

            _velocity *= Mathf.Exp(-friction * Time.deltaTime);
        }
        else
        {
            _velocity = Vector3.zero;
        }

        _lastWorldPos = transform.position;
    }

    protected override void OnGrabStarted(PointerEventData e)
    {
        if (GetComponent<Banknote>() != null)
        {
            ReparentToBanknotesParent();
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, e.position, e.pressEventCamera, out Vector2 local);

        _localPoint = transform.TransformVector(local);

        _velocity = Vector3.zero;

        _lastWorldPos = transform.position;

        if (!string.IsNullOrEmpty(pickSound) && AudioManager.Instance != null)
            AudioManager.Instance.Play(pickSound);
    }

    protected override void OnGrabEnded(GrabEndReason reason)
    {
        if (reason == GrabEndReason.Released &&
            !string.IsNullOrEmpty(dropSound) && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(dropSound);
        }
    }

    protected override void OnGrabCancelled()
    {
        _velocity = Vector3.zero;
        _lastWorldPos = transform.position;
    }

    private void ReparentToBanknotesParent()
    {
        Transform target = FindBanknotesParent();
        if (target == null)
        {
            Debug.LogWarning($"No object found with tag '{banknotesParentTag}'", this);
            return;
        }

        if (transform.parent == target) return;

        transform.SetParent(target, true);
    }

    private Transform FindBanknotesParent()
    {
        var go = GameObject.FindGameObjectWithTag(banknotesParentTag);
        return go != null ? go.transform : null;
    }
}