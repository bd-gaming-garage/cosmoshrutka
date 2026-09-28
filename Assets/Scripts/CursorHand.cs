using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class CursorHand : MonoBehaviour
{
    RectTransform rt;
    RectTransform parentRect;
    Canvas canvas;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        parentRect = canvas.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, mousePos, canvas.worldCamera, out Vector2 localPoint);
        rt.anchoredPosition = localPoint;
    }
}