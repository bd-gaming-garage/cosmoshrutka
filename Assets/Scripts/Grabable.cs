using UnityEngine;
using UnityEngine.EventSystems;

public class Grabable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _isGrabbed = false;
    Vector2 localPoint;
    [SerializeField] private CursorHand hand;
    [SerializeField] private int value;
    private RectTransform _rt;

    [SerializeField] private string pickSound = null;
    [SerializeField] private string dropSound = null;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }


    void Start()
    {
        hand = CursorHand.Instance;
    }

    private void LateUpdate()
    {
        if (!_isGrabbed) return;
        transform.position = hand.transform.position - (Vector3) localPoint;
    }

    public void OnPointerDown(PointerEventData e)
    {
        DetachFromPassengerHand();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, e.position, e.pressEventCamera, out Vector2 local);

        localPoint = transform.TransformVector(local);

        hand.Grab(gameObject);
        Grab();
    }

    public void OnPointerUp(PointerEventData e)
    {
        hand.Ungrab();
    }

    public void Grab() {
        _isGrabbed = true;

        if (pickSound != null)
            AudioManager.Instance.Play(pickSound);
    }

    public void Ungrab() {
        if (!_isGrabbed) return;
        _isGrabbed = false;


        if (dropSound != null)
            AudioManager.Instance.Play(dropSound);
    }

    private void DetachFromPassengerHand()
    {
        Transform parent = transform.parent;
        if (parent == null) return;

        // проверяем, есть ли PassengerHand у родителя или выше по иерархии
        bool insidePassengerHand =
            parent.GetComponentInParent<PassengerHand>() != null;

        if (!insidePassengerHand) return;

        var canvas = GetComponentInParent<Canvas>();
        Transform target = canvas != null ? canvas.transform : null;

        // worldPositionStays = true — позиция/поворот/масштаб сохранятся
        transform.SetParent(target, true);
    }
}
