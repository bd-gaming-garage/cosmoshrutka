using System.Collections.Generic;
using UnityEngine;

public class PassangersManager : MonoBehaviour
{
    [Header("Точки спавна")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Что спавнить")]
    [SerializeField] private GameObject prefab;

    [SerializeField] private Transform parent;

    [Header("Настройки")]
    [Tooltip("Радиус проверки — если в этой зоне уже есть объект, точка считается занятой")]
    [SerializeField] private float checkRadius = 1f;
    [Tooltip("Слой объектов, которые считаются 'занятыми'")]
    [SerializeField] private LayerMask occupiedMask = ~0; // по умолчанию всё

    // запоминаем, что уже заспавнили, чтобы не плодить дубликаты
    private readonly HashSet<Transform> usedPoints = new HashSet<Transform>();

    private void Start()
    {
        SpawnAll();
    }

    /// <summary>Заспавнить во всех свободных точках.</summary>
    public void SpawnAll()
    {
        foreach (var point in spawnPoints)
        {
            if (point == null) continue;
            if (usedPoints.Contains(point)) continue;   // уже занята нами
            if (IsOccupied(point)) continue;            // занята кем-то ещё

            SpawnAt(point);
        }
    }

    public GameObject SpawnAt(Transform point)
    {
        if (point == null || prefab == null) return null;

        Transform actualParent = parent != null ? parent : transform;

        var obj = Instantiate(prefab, point.position, point.rotation, actualParent);

        usedPoints.Add(point);
        return obj;
    }

    /// <summary>Проверка, занята ли точка.</summary>
    private bool IsOccupied(Transform point)
    {
        Collider[] hits = Physics.OverlapSphere(point.position, checkRadius, occupiedMask);
        foreach (var hit in hits)
        {
            // игнорируем триггеры и сами точки спавна, если они с коллайдерами
            if (hit.isTrigger) continue;
            if (hit.transform == point) continue;
            return true;
        }
        return false;
    }

    // Наглядная визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        foreach (var point in spawnPoints)
        {
            if (point == null) continue;

            Gizmos.color = (usedPoints != null && usedPoints.Contains(point))
                ? Color.red      // занята нами
                : Color.green;   // свободна

            Gizmos.DrawWireSphere(point.position, checkRadius);
        }
    }
}
