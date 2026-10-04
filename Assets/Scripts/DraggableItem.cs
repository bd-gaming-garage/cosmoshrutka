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

    [Header("Screen bounds")]
    [Tooltip("Fraction of velocity retained after hitting a screen edge")]
    [SerializeField, Range(0f, 1f)] private float bounceRetention = 0.65f;

    private Canvas _rootCanvas;
    private readonly Vector3[] _worldCorners = new Vector3[4];

    private Vector3 _lastWorldPos;
    private Vector3 _velocity;

    protected override CursorHand ResolveHand() =>
        hand != null ? hand : base.ResolveHand();

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        CacheCanvas();
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

        BounceInsideCanvas();
        _lastWorldPos = transform.position;
    }

    private void OnTransformParentChanged()
    {
        CacheCanvas();
    }

    private void CacheCanvas()
    {
        var canvas = GetComponentInParent<Canvas>();
        _rootCanvas = canvas != null ? canvas.rootCanvas : null;
    }

    private void BounceInsideCanvas()
    {
        // Screen-space Canvas bounds follow the screen or camera viewport.
        if (_rootCanvas == null || _rootCanvas.renderMode == RenderMode.WorldSpace)
            return;

        var canvasRect = (RectTransform)_rootCanvas.transform;
        _rt.GetWorldCorners(_worldCorners);

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in _worldCorners)
        {
            Vector2 local = canvasRect.InverseTransformPoint(corner);
            min = Vector2.Min(min, local);
            max = Vector2.Max(max, local);
        }

        Rect bounds = canvasRect.rect;
        Vector3 localVelocity = canvasRect.InverseTransformVector(_velocity);
        Vector3 correction = Vector3.zero;
        correction.x = ResolveBoundary(
            min.x, max.x, bounds.xMin, bounds.xMax, ref localVelocity.x);
        correction.y = ResolveBoundary(
            min.y, max.y, bounds.yMin, bounds.yMax, ref localVelocity.y);

        transform.position += canvasRect.TransformVector(correction);
        _velocity = canvasRect.TransformVector(localVelocity);
    }

    private float ResolveBoundary(
        float min, float max, float lower, float upper, ref float velocity)
    {
        if (max - min >= upper - lower)
        {
            // An oversized item cannot fit on this axis; keep it centered.
            velocity = 0f;
            return (lower + upper - min - max) * 0.5f;
        }

        float retention = Mathf.Clamp01(bounceRetention);
        if (min <= lower)
        {
            if (velocity < 0f) velocity = -velocity * retention;
            return lower - min;
        }

        if (max >= upper)
        {
            if (velocity > 0f) velocity = -velocity * retention;
            return upper - max;
        }

        return 0f;
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