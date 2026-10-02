using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PassengerHand : MonoBehaviour
{
    [Header("Движение руки 'назад' (в UI-координатах)")] [SerializeField]
    private Vector2 backDirection = new Vector2(-1f, 0f);

    [SerializeField] private float backDistance = 400f;
    [SerializeField] private float pullDuration = 0.2f;
    [SerializeField] private float backDuration = 0.6f;
    [SerializeField] private AnimationCurve backCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Звук (необязательно)")] [SerializeField]
    private string grabSound = "";

    private RectTransform _rt;
    [SerializeField] private bool triggered;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (triggered) return;

        if (GetComponentInChildren<Banknote>(true) != null) return;

        var coin = FindCoinOverlappingHand();
        if (coin == null) return;

        triggered = true;
        StartCoroutine(TakeCoin(coin));
    }

    private Coin FindCoinOverlappingHand()
    {
        var coins = FindObjectsByType<Coin>();
        if (coins.Length == 0) return null;

        Rect handRect = GetWorldRect(_rt);

        foreach (var coin in coins)
        {
            if (coin == null) continue;

            if (!coin.active) continue;

            var coinRt = coin.GetComponent<RectTransform>();
            if (coinRt == null) continue;

            Rect coinRect = GetWorldRect(coinRt);

            if (handRect.Overlaps(coinRect))
                return coin;
        }

        return null;
    }

    private static Rect GetWorldRect(RectTransform rt)
    {
        if (rt == null) return default;

        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);

        float minX = Mathf.Min(c[0].x, c[1].x, c[2].x, c[3].x);
        float maxX = Mathf.Max(c[0].x, c[1].x, c[2].x, c[3].x);
        float minY = Mathf.Min(c[0].y, c[1].y, c[2].y, c[3].y);
        float maxY = Mathf.Max(c[0].y, c[1].y, c[2].y, c[3].y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private IEnumerator TakeCoin(Coin coin)
    {
        coin.active = false;

        var coinRb = coin.GetComponent<Rigidbody2D>();
        if (coinRb != null) coinRb.simulated = false;

        var grab = coin.GetComponent<DraggableItem>();
        if (grab != null) grab.enabled = false;
        
        var hand = CursorHand.Instance;
        if (hand != null && hand.grabbedObject == coin.gameObject)
        {
            hand.Ungrab();
        }

        if (!string.IsNullOrEmpty(grabSound) && AudioManager.Instance != null)
            AudioManager.Instance.Play(grabSound);

        RectTransform coinRt = coin.GetComponent<RectTransform>();

        Vector3 coinStartWorld = coinRt.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / pullDuration;
            coinRt.position = Vector3.Lerp(coinStartWorld, _rt.position, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        coinRt.SetParent(_rt, true);

        Vector2 handStart = _rt.anchoredPosition;

        float z = _rt.localEulerAngles.z;
        Vector2 rotatedDir = Quaternion.Euler(0f, 0f, z) * backDirection.normalized;
        Vector2 handEnd = handStart + rotatedDir * backDistance;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / backDuration;
            float k = backCurve.Evaluate(Mathf.Clamp01(t));
            _rt.anchoredPosition = Vector2.Lerp(handStart, handEnd, k);
            yield return null;
        }

        Destroy(coin.gameObject);
        Destroy(gameObject);
    }
}