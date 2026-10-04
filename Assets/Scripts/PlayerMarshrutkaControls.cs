using UnityEngine;

public class PlayerMarshrutkaControls : MonoBehaviour
{
    [SerializeField] private MarshrutkaController marshrutka;
    [SerializeField] private SteeringWheel steeringWheel;

    [SerializeField] private Pedal throttlePedal;
    [SerializeField] private Pedal brakePedal;

    public bool TrySelectGear(MarshrutkaGear gear)
    {
        return marshrutka != null && marshrutka.SetInput(CreateInput(gear));
    }

    public void SelectDrive()
    {
        TrySelectGear(MarshrutkaGear.Drive);
    }

    public void SelectReverse()
    {
        TrySelectGear(MarshrutkaGear.Reverse);
    }

    private void LateUpdate()
    {
        if (marshrutka != null)
        {
            marshrutka.SetInput(CreateInput(marshrutka.CurrentGear));
        }
    }

    private MarshrutkaInput CreateInput(MarshrutkaGear gear)
    {
        float steering = steeringWheel != null
            ? steeringWheel.SteeringInput : 0f;

        float throttle = throttlePedal != null && throttlePedal.isActiveAndEnabled
            ? throttlePedal.PressAmount : 0f;

        float brake = brakePedal != null && brakePedal.isActiveAndEnabled
            ? brakePedal.PressAmount : 0f;

        return new MarshrutkaInput(steering, throttle, brake, gear);
    }

    private void ResetPedals()
    {
        if (throttlePedal != null)
        {
            throttlePedal.ResetPress();
        }
        if (brakePedal != null)
        {
            brakePedal.ResetPress();
        }

        if (marshrutka != null)
        {
            marshrutka.SetInput(new MarshrutkaInput(0f, 0f, 0f, marshrutka.CurrentGear));
        }
    }

    private void OnDisable()
    {
        ResetPedals();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ResetPedals();
        }
    }
}