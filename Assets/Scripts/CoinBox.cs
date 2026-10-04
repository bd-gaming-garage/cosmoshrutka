using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CoinBox : MonoBehaviour, IGrabTarget
{
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private RectTransform coinsParent;

    private int _lastDispenseFrame = -1;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || !Application.isFocused ||
            eventData.button != PointerEventData.InputButton.Left ||
            _lastDispenseFrame == Time.frameCount)
        {
            return;
        }

        var hand = CursorHand.Instance;
        if (hand == null || !hand.isActiveAndEnabled || hand.IsGrabbing)
            return;

        if (coinPrefab == null || coinsParent == null ||
            !coinsParent.gameObject.activeInHierarchy)
        {
            return;
        }

        if (!coinPrefab.TryGetComponent<DraggableItem>(out var prefabItem) ||
            !prefabItem.enabled || !coinPrefab.gameObject.activeSelf)
        {
            Debug.LogWarning(
                "Coin prefab must be active and have an enabled DraggableItem.",
                this);
            return;
        }

        var canvas = coinsParent.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                coinsParent, eventData.position, camera,
                out Vector3 spawnPosition))
        {
            return;
        }

        var coin = Instantiate(coinPrefab, coinsParent, false);
        coin.transform.position = spawnPosition;
        coin.transform.SetAsLastSibling();
        coin.active = true;

        var item = coin.GetComponent<DraggableItem>();
        item.OnPointerDown(eventData);

        if (hand.GrabbedObject != coin.gameObject)
        {
            // Do not leave a spare coin if grabbing failed.
            coin.active = false;
            coin.gameObject.SetActive(false);
            Destroy(coin.gameObject);
            return;
        }

        _lastDispenseFrame = Time.frameCount;
    }
}
