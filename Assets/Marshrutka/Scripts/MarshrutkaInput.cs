namespace Marshrutka.Scripts
{
    public readonly struct MarshrutkaInput
    {
        public float Steering { get; }
        public float Throttle { get; }
        public float Brake { get; }
        public MarshrutkaGear Gear { get; }

        public MarshrutkaInput(
            float steering,
            float throttle,
            float brake,
            MarshrutkaGear gear)
        {
            Steering = steering;
            Throttle = throttle;
            Brake = brake;
            Gear = gear;
        }
    }
}