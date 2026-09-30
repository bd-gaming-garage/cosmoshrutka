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
        if (hand == null)
            hand = GameObject.FindGameObjectWithTag("CursorHand").GetComponent<CursorHand>();
    }

    private void LateUpdate()
    {
        if (!_isGrabbed) return;
        transform.position = hand.transform.position - (Vector3) localPoint;
    }

    public void OnPointerDown(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, e.position, e.pressEventCamera, out Vector2 local);

        localPoint = local;

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
}
