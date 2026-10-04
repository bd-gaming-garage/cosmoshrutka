using UnityEngine;

public class BusStop : MonoBehaviour
{
    [SerializeField] private string _busTag = "Bus";
    [SerializeField] private uint _passengers = 10;
    [SerializeField] private uint _passengersMax = 10;
    [SerializeField] private uint _passengersPerTick = 1;
    [SerializeField] private float _tickTime = 4f;

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _tickTime)
        {
            _timer -= _tickTime;

            IncrementPassangers(_passengersPerTick);
        }
    }

    private void IncrementPassangers(uint n)
    {
        if (_passengers + n > _passengersMax)
        {
            _passengers = _passengersMax;
        }
        else
        {
            _passengers += n;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_busTag)) return;

        Trigger();
    }

    private void Trigger()
    {
        PassengerSpawner.Instance.passengersServed = 0;
        _passengers = PassengerSpawner.Instance.IncreacePassangers(_passengers);
    }
}
