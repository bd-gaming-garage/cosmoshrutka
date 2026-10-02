using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class CursorHand : MonoBehaviour
{
    public static CursorHand Instance;

    public bool isGrabbing = false;
    [SerializeField] public GameObject grabbedObj;
    private Vector2 _handPos;
    private float _catchUp;
    [SerializeField] private float catchUpTime = 0.15f;

    [SerializeField] private GameObject steeringWheel;

    private void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (isGrabbing && Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Ungrab();
        }

        if (isGrabbing && grabbedObj == steeringWheel)
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
    }

    public void Grab(GameObject obj) {
        if (isGrabbing) return;

        grabbedObj = obj;
        isGrabbing = true;
    }

    public void Ungrab() {
        if (!isGrabbing) return;

        isGrabbing = false;

        if (grabbedObj != null && grabbedObj != steeringWheel)
            grabbedObj.GetComponent<Grabable>().Ungrab();
    }
}
