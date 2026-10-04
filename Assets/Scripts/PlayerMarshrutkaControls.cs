using UnityEngine;

public class PlayerMarshrutkaControls : MonoBehaviour
{
    [SerializeField] private MarshrutkaController marshrutka;
    [SerializeField] private SteeringWheel steeringWheel;

    [SerializeField] private float throttle;
    [SerializeField] private float brake;

    public void SetThrottle(float value)
    {
        throttle = Mathf.Clamp01(value);
    }

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
        
    public void SetBrake(float value)
    {
        brake = Mathf.Clamp01(value);
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
            ? steeringWheel.SteeringInput
            : 0f;

        return new MarshrutkaInput(steering, throttle, brake, gear);
    }

    private void ResetPedals()
    {
        throttle = 0f;
        brake = 0f;

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