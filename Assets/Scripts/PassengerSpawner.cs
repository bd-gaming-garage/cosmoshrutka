using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("Spawn points")] [SerializeField]
    private Transform[] spawnPoints;

    [Header("Passenger prefab")] [SerializeField]
    private GameObject prefab;

    [SerializeField] private Transform parent;

    [Header("Editor visualization")] [Tooltip("Spawn point marker radius")] [SerializeField]
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
