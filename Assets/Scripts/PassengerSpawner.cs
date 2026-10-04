using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    public static PassengerSpawner Instance;

    [Header("Spawn points")] [SerializeField]
    private Transform[] spawnPoints;

    [Header("Passenger prefab")] [SerializeField]
    private GameObject prefab;

    [SerializeField] private Transform parent;

    [Header("Editor visualization")] [Tooltip("Spawn point marker radius")] [SerializeField]
    private float checkRadius = 1f;

    private readonly Dictionary<Transform, GameObject> passengersByPoint = new();

    [Header("Passangers")]
    [SerializeField] private uint passengersMax = 30;
    [SerializeField] private uint passengersNotServed = 0;
    [SerializeField] private uint passengersServed = 0;
    [SerializeField] private uint passengersServing = 0;


    private void Awake()
    {
        Instance = this;
    }

    private uint AllPassengers() {
        return passengersNotServed + passengersServed + passengersServing;
    }

    public uint IncreacePassangers(uint n)
    {
        if (AllPassengers() + n <= passengersMax) {
            passengersNotServed += n;
            return 0;
        }

        uint passengersStay = AllPassengers() + n - passengersMax;
        passengersNotServed = passengersMax - passengersNotServed - passengersServing;
        return passengersStay;
    }

    public void ServePassanger()
    {
        passengersServed++;
    }

    private void Update()
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
        if (passengersNotServed == 0) {
            return null;
        } 

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

        passengersNotServed -= 1;
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
