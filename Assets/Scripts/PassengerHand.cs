using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Collider2D))]
public class PassengerHand : MonoBehaviour
{
    [Header("Движение руки 'назад' (в UI-координатах)")]
    [Tooltip("Направление 'назад'. По умолчанию влево. Для вправо — (1,0), вверх — (0,1), вниз — (0,-1)")]
    [SerializeField] private Vector2 backDirection = new Vector2(-1f, 0f);
    [SerializeField] private float backDistance = 400f;   // в пикселях UI
    [SerializeField] private float pullDuration = 0.2f;
    [SerializeField] private float backDuration = 0.6f;
    [SerializeField] private AnimationCurve backCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Звук (необязательно)")]
    [SerializeField] private string grabSound = "";

    private CursorHand hand;

    private RectTransform _rt;
    [SerializeField] private bool triggered;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    private void Start()
    {
        hand = CursorHand.Instance;
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        var coin = other.GetComponent<Coin>();
        if (coin == null) return;

        triggered = true;
        StartCoroutine(TakeCoin(coin));
    }

    private IEnumerator TakeCoin(Coin coin)
    {

        hand.Ungrab();
        // Отключаем взаимодействие с монетой
        var coinCol = coin.GetComponent<Collider2D>();
        if (coinCol != null) coinCol.enabled = false;

        var coinRb = coin.GetComponent<Rigidbody2D>();
        if (coinRb != null) coinRb.simulated = false;

        var grab = coin.GetComponent<Grabable>();
        if (grab != null) grab.enabled = false;

        if (!string.IsNullOrEmpty(grabSound) && AudioManager.Instance != null)
            AudioManager.Instance.Play(grabSound);

        RectTransform coinRt = coin.GetComponent<RectTransform>();

        // 1. Монета летит к руке (в мировых координатах, чтобы не зависеть от родителя)
        Vector3 coinStartWorld = coinRt.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / pullDuration;
            coinRt.position = Vector3.Lerp(coinStartWorld, _rt.position, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Монета становится дочерней — дальше едет вместе с рукой
        coinRt.SetParent(_rt, true);

        // 2. Рука уезжает "назад" в UI-координатах (anchoredPosition)
        Vector2 handStart = _rt.anchoredPosition;
        Vector2 handEnd = handStart + backDirection.normalized * backDistance;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / backDuration;
            float k = backCurve.Evaluate(Mathf.Clamp01(t));
            _rt.anchoredPosition = Vector2.Lerp(handStart, handEnd, k);
            yield return null;
        }

        // 3. Исчезаем — рука и монета
        Destroy(coin.gameObject);
        Destroy(gameObject);
    }
}