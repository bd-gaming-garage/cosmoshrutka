using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Follow the hand after CursorHand.LateUpdate, before ClickGrabSelector.
[DefaultExecutionOrder(50)]
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

    private const float ThrowSampleWindow = 0.08f;
    private readonly Queue<MotionSample> _motionSamples = new Queue<MotionSample>(16);
    private CursorHand _trackedHand;
    private Vector3 _velocity;
    private bool _isReleased;
    private bool _releasePending;

    private readonly struct MotionSample
    {
        public readonly Vector3 Position;
        public readonly float Time;

        public MotionSample(Vector3 position, float time)
        {
            Position = position;
            Time = time;
        }
    }

    protected override CursorHand ResolveHand() =>
        hand != null ? hand : base.ResolveHand();

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        CacheCanvas();
    }

    private void LateUpdate()
    {
        if (IsGrabbed)
        {
            FollowHandAndSampleVelocity();
            return;
        }

        if (_releasePending)
        {
            // Release callbacks run before the hand's final LateUpdate movement.
            FollowHandAndSampleVelocity();
            _releasePending = false;
            _trackedHand = null;
            _motionSamples.Clear();
            BounceInsideCanvas();
            return;
        }

        // Items still held by passengers must follow their parent animation.
        if (!_isReleased) return;

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
    }

    private void FollowHandAndSampleVelocity()
    {
        if (_trackedHand == null)
        {
            _velocity = Vector3.zero;
            return;
        }

        Vector3 handPosition = _trackedHand.transform.position;
        transform.position = handPosition - (Vector3)_localPoint;

        float now = Time.time;
        _motionSamples.Enqueue(new MotionSample(handPosition, now));

        // Retain at least two samples, even when a frame exceeds the window.
        while (_motionSamples.Count > 2 &&
               (now - _motionSamples.Peek().Time > ThrowSampleWindow ||
                _motionSamples.Count > 64))
        {
            _motionSamples.Dequeue();
        }

        MotionSample first = _motionSamples.Peek();
        float elapsed = now - first.Time;
        _velocity = elapsed > 0.0001f
            ? Vector3.ClampMagnitude(
                (handPosition - first.Position) / elapsed, Mathf.Max(maxVelocity, 0f))
            : Vector3.zero;
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
        _isReleased = false;
        _releasePending = false;
        _motionSamples.Clear();
        _trackedHand = GrabHand;
        if (GetComponent<Banknote>() != null)
        {
            ReparentToBanknotesParent();
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, e.position, e.pressEventCamera, out Vector2 local);

        _localPoint = transform.TransformVector(local);

        _velocity = Vector3.zero;

        _motionSamples.Enqueue(new MotionSample(_trackedHand.transform.position, Time.time));

        if (!string.IsNullOrEmpty(pickSound) && AudioManager.Instance != null)
            AudioManager.Instance.Play(pickSound);
    }

    protected override void OnGrabEnded(GrabEndReason reason)
    {
        _isReleased = reason == GrabEndReason.Released;
        _releasePending = _isReleased;
        if (reason == GrabEndReason.Released &&
            !string.IsNullOrEmpty(dropSound) && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(dropSound);
        }
    }

    protected override void OnGrabCancelled()
    {
        _isReleased = false;
        _releasePending = false;
        _trackedHand = null;
        _motionSamples.Clear();
        _velocity = Vector3.zero;
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