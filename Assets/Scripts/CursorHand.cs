using Interaction.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Scripts
{
    [RequireComponent(typeof(RectTransform))]
    public class CursorHand : MonoBehaviour
    {
        public static CursorHand Instance;

        public GameObject GrabbedObject { get; private set; }
        public bool IsGrabbing => GrabbedObject != null;
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
            var mouse = Mouse.current;
            if (mouse == null)
            {
                Ungrab();
                return;
            }

            if (IsGrabbing && mouse.leftButton.wasReleasedThisFrame)
            {
                Ungrab();
            }

            Vector2 mousePos = mouse.position.ReadValue();

            if (IsGrabbing && GrabbedObject == steeringWheel)
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

        public bool TryGrab(GameObject targetObject)
        {
            if (IsGrabbing || targetObject == null)
            {
                return false;
            }

            GrabbedObject = targetObject;
            return true;
        }

        public void Ungrab()
        {
            GameObject releasedObject = GrabbedObject;
            GrabbedObject = null;

            if (releasedObject == null)
            {
                return;
            }

            if (releasedObject.TryGetComponent<IGrabbable>(out var item))
            {
                item.ReleaseGrab();
            }
        }
    }
}