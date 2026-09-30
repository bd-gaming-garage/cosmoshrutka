using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class CursorHand : MonoBehaviour
{
    RectTransform rt;
    RectTransform parentRect;
    Canvas canvas;

    public bool isGrabbing = false;
    [SerializeField] private GameObject grabbedObj;
    private Vector2 _handPos;
    private float _catchUp;
    [SerializeField] private float catchUpTime = 0.15f;


    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        parentRect = canvas.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (isGrabbing)
        {
            _handPos = transform.position;
            _catchUp = 1f;
            return;
        }

        if (_catchUp > 0f)
        {
            _catchUp -= Time.deltaTime / catchUpTime;
            _handPos = Vector2.Lerp(mousePos, _handPos, Mathf.Max(_catchUp, 0f));
            transform.position = _handPos;
        }
        else
        {
            transform.position = mousePos;
        }

        if (isGrabbing) {
            grabbedObj.transform.position = transform.position;
        }
    }

    public void Grab(GameObject obj) {
        if (isGrabbing) return;

        isGrabbing = true;
        grabbedObj = obj;
    }

    public void Ungrab() {
        if (!isGrabbing) return;

        isGrabbing = false;
    }
}
