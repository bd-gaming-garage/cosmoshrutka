using System.Collections;
using UnityEngine;

namespace Passengers.Scripts
{
    public class PassengerEntranceAnimation : MonoBehaviour
    {
        [SerializeField] private float backOffset = 5f;

        [SerializeField] private float duration = 1f;

        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 targetPosition;

        public bool IsComplete { get; private set; }

        private void Start()
        {
            targetPosition = transform.position;

            if (duration <= 0f)
            {
                IsComplete = true;
                return;
            }

            transform.position = targetPosition - transform.right * backOffset;

            StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            Vector3 startPos = transform.position;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float k = curve.Evaluate(Mathf.Clamp01(t));
                transform.position = Vector3.Lerp(startPos, targetPosition, k);
                yield return null;
            }

            transform.position = targetPosition;
            IsComplete = true;
        }
    }
}