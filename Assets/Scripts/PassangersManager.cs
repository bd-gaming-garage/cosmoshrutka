using System.Collections.Generic;
using UnityEngine;

public class PassangersManager : MonoBehaviour
{
    [Header("Точки спавна")] [SerializeField]
    private Transform[] spawnPoints;

    [Header("Что спавнить")] [SerializeField]
    private GameObject prefab;

    [SerializeField] private Transform parent;

    [Header("Отображение в редакторе")] [Tooltip("Радиус маркера точки спавна")] [SerializeField]
    private float checkRadius = 1f;

    private readonly Dictionary<Transform, GameObject> passengersByPoint = new();

    private void Start()
    {
        SpawnAll();
    }


    public void SpawnAll()
    {
        if (spawnPoints == null)
        {
            return;
        }

        foreach (var point in spawnPoints)
        {
            SpawnAt(point);
        }
    }

    public GameObject SpawnAt(Transform point)
    {
        if (point == null || prefab == null)
        {
            return null;
        }

        if (passengersByPoint.TryGetValue(point, out var passenger) && passenger != null)
        {
            return null;
        }

        Transform actualParent = parent != null ? parent : transform;

        var obj = Instantiate(prefab, point.position, point.rotation, actualParent);

        passengersByPoint[point] = obj;
        return obj;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        foreach (var point in spawnPoints)
        {
            if (point == null) continue;

            Gizmos.color = (passengersByPoint.TryGetValue(point, out var passenger) && passenger != null)
                ? Color.red
                : Color.green;

            Gizmos.DrawWireSphere(point.position, checkRadius);
        }
    }
}