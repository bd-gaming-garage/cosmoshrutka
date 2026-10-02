using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ClickRadius : MonoBehaviour
{
    [SerializeField] private float radius = 100f;

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (CursorHand.Instance == null || CursorHand.Instance.isGrabbing) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        MonoBehaviour nearest = null;
        float best = radius;

        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is not IPointerDownHandler handler) continue;
            if (!mb.enabled) continue;

            var rt = mb.transform as RectTransform;
            if (rt == null) continue;

            float d = Vector2.Distance(mousePos, RectTransformUtility.WorldToScreenPoint(null, rt.position));
            if (d <= best) { best = d; nearest = mb; }
        }

        if (nearest == null) return;

        var ped = new PointerEventData(EventSystem.current) { position = mousePos };
        ((IPointerDownHandler)nearest).OnPointerDown(ped);
    }
}