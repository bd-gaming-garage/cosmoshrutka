using System.Collections;
using UnityEngine;

public class SpawnMove : MonoBehaviour
{
    [Header("Отступ назад (в локальных осях объекта)")]
    [SerializeField] private float backOffset = 5f;

    [Header("Скорость движения к цели")]
    [SerializeField] private float duration = 1f;

    [Header("Кривая (плавность)")]
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 targetPosition;

    private void Start()
    {
        // целевая точка — та, куда поставил спавнер
        targetPosition = transform.position;

        // телепорт "назад" относительно ПОВОРОТА объекта
        transform.position = targetPosition - transform.right * backOffset;
        // ↑ если "назад" по другой оси — см. ниже

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
    }
}