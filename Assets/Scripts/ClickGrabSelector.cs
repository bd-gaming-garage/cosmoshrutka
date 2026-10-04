using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
public class ClickGrabSelector : MonoBehaviour
{
    [SerializeField] private GrabArea grabArea;
    private readonly List<RaycastResult> hits = new();

    private void LateUpdate()
    {
        if (grabArea == null || !grabArea.isActiveAndEnabled)
        {
            return;
        }

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (CursorHand.Instance == null || CursorHand.Instance.IsGrabbing)
        {
            return;
        }

        if (EventSystem.current == null)
        {
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();

        var hand = CursorHand.Instance;
        var handCanvas = hand.GetComponentInParent<Canvas>();
        if (handCanvas == null) return;

        Camera handCamera = handCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : handCanvas.worldCamera;

        Vector2 grabPoint = RectTransformUtility.WorldToScreenPoint(
            handCamera, hand.transform.position);

        var ped = new PointerEventData(EventSystem.current)
        {
            position = mousePos,
            button = PointerEventData.InputButton.Left
        };

        if (FindTopHandler(ped) != null)
        {
            return;
        }

        MonoBehaviour nearest = null;
        RaycastResult nearestHit = default;
        float best = float.PositiveInfinity;

        foreach (var behaviour in FindObjectsByType<MonoBehaviour>())
        {
            if (behaviour is not IGrabTarget)
            {
                continue;
            }

            if (!behaviour.isActiveAndEnabled)
            {
                continue;
            }

            var rt = behaviour.transform as RectTransform;
            if (rt == null) continue;

            var canvas = behaviour.GetComponentInParent<Canvas>();
            if (canvas == null) continue;

            // This area operates in the same Canvas coordinate space.
            if (canvas.rootCanvas != handCanvas.rootCanvas) continue;
            if (!grabArea.Overlaps(rt)) continue;

            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, rt.TransformPoint(rt.rect.center));

            float distance = Vector2.Distance(grabPoint, center);
            if (distance <= best)
            {
                ped.position = center;

                if (FindTopHandler(ped) != behaviour.gameObject)
                {
                    continue;
                }

                best = distance;
                nearest = behaviour;
                nearestHit = hits[0];
            }
        }

        if (nearest == null) return;

        ped.position = mousePos;
        ped.pressPosition = mousePos;
        ped.pointerPressRaycast = nearestHit;

        ((IPointerDownHandler)nearest).OnPointerDown(ped);
    }

    private GameObject FindTopHandler(PointerEventData eventData)
    {
        hits.Clear();
        EventSystem.current.RaycastAll(eventData, hits);

        return hits.Count == 0 ? null : ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject);
    }
}