using Audio.Scripts;
using Money.Scripts;
using Player.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction.Scripts
{
    public class DraggableItem : MonoBehaviour, IGrabbable, IPointerDownHandler, IPointerUpHandler
    {
        private bool _isGrabbed = false;
        Vector2 localPoint;
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

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        void Start()
        {
            hand = CursorHand.Instance;
            _lastWorldPos = transform.position;
        }

        private void LateUpdate()
        {
            if (_isGrabbed)
            {
                transform.position = hand.transform.position - (Vector3)localPoint;

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

        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left)
                return;

            if (!hand.TryGrab(gameObject))
            {
                return;
            }

            if (GetComponent<Banknote>() != null)
            {
                ReparentToBanknotesParent();
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rt, e.position, e.pressEventCamera, out Vector2 local);

            localPoint = transform.TransformVector(local);

            _velocity = Vector3.zero;

            Grab();
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left)
                return;

            if (hand.GrabbedObject == gameObject)
                hand.Ungrab();
        }

        public void Grab()
        {
            _isGrabbed = true;

            if (pickSound != null)
                AudioManager.Instance.Play(pickSound);
        }

        public void ReleaseGrab()
        {
            if (!_isGrabbed) return;
            _isGrabbed = false;

            if (!string.IsNullOrEmpty(dropSound))
                AudioManager.Instance.Play(dropSound);
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
}
