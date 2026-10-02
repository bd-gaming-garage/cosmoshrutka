using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MoneyDrop : MonoBehaviour
{
    [Header("Коробка (перетащи сюда RectTransform коробки)")]
    [SerializeField] private RectTransform boxRect;

    [Header("Зона срабатывания (отступ вокруг коробки)")]
    [SerializeField] private float boxPadding = 30f;

    [Header("Анимация")]
    [SerializeField] private float riseHeight = 90f;      // на сколько поднимется
    [SerializeField] private float riseDuration = 0.25f;  // время подъёма
    [SerializeField] private float floatDuration = 0.35f; // время левитации
    [SerializeField] private float flyDuration = 0.45f;   // время полёта в коробку
    [SerializeField] private float wobbleAmount = 6f;     // покачивание
    [SerializeField] private AnimationCurve flyCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 endScale = new Vector3(0.3f, 0.3f, 1f);

    [Header("Звук (необязательно)")]
    [SerializeField] private string collectSound = "";

    private CursorHand hand;
    private bool wasGrabbing;
    private bool triggered;

    private void Start()
    {
        hand = CursorHand.Instance;
        boxRect = BanknoteBox.Instance.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (triggered || hand == null || boxRect == null) return;

        if (IsOverBox())
        {
            Trigger();
        }
        wasGrabbing = hand.isGrabbing;
    }

    private bool IsOverBox()
    {
        Vector3[] c = new Vector3[4];
        boxRect.GetWorldCorners(c);

        Vector3 min = c[0] - new Vector3(boxPadding, boxPadding, 0f);
        Vector3 max = c[2] + new Vector3(boxPadding, boxPadding, 0f);

        Vector3 p = transform.position;
        return p.x >= min.x && p.x <= max.x && p.y >= min.y && p.y <= max.y;
    }

    private void Trigger()
    {
        triggered = true;

        var grabable = GetComponent<Grabable>();
        if (grabable != null) grabable.enabled = false;

        if (!string.IsNullOrEmpty(collectSound) && AudioManager.Instance != null)
            AudioManager.Instance.Play(collectSound);

        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        RectTransform rt = (RectTransform)transform;
        Vector3 startPos = rt.position;
        Vector3 risePos = startPos + Vector3.up * riseHeight;
        Vector3 targetPos = boxRect.position;
        Vector3 startScale = rt.localScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / riseDuration;
            rt.position = Vector3.Lerp(startPos, risePos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        t = 0f;
        Vector3 from = rt.position;
        while (t < 1f)
        {
            t += Time.deltaTime / flyDuration;
            float k = flyCurve.Evaluate(Mathf.Clamp01(t));
            rt.position = Vector3.Lerp(from, targetPos, k);
            rt.localScale = Vector3.Lerp(startScale, endScale, k);
            yield return null;
        }

        Destroy(gameObject);
    }
}