using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickGrabSelector : MonoBehaviour
{
    [SerializeField] private float radius = 40f;
    private readonly List<RaycastResult> hits = new();

    private void Update()
    {
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
        float best = radius;

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

            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, rt.TransformPoint(rt.rect.center));

            float distance = Vector2.Distance(mousePos, center);
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