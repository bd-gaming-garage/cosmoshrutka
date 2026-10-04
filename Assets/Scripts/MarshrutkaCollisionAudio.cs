using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MarshrutkaCollisionAudio : MonoBehaviour
{
    [SerializeField] private string lightImpactSound = "BusImpactLight";
    [SerializeField] private string heavyImpactSound = "BusImpactHeavy";

    [Tooltip("Minimum normal impact speed in m/s to play a sound")]
    [SerializeField, Min(0.01f)] private float minImpactSpeed = 0.5f;

    [Tooltip("Normal impact speed in m/s that selects the heavy sound")]
    [SerializeField, Min(0.01f)] private float heavyImpactSpeed = 6f;

    [Tooltip("Minimum time between impact sounds in seconds")]
    [SerializeField, Min(0f)] private float cooldown = 0.15f;

    private float _nextSoundTime;

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActiveAndEnabled || Time.time < _nextSoundTime)
            return;

        var audio = AudioManager.Instance;
        if (audio == null) return;

        float impactSpeed = 0f;
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float normalSpeed = Mathf.Abs(
                Vector3.Dot(collision.relativeVelocity, normal));

            impactSpeed = Mathf.Max(impactSpeed, normalSpeed);
        }

        if (impactSpeed < minImpactSpeed) return;

        string sound = impactSpeed >= Mathf.Max(heavyImpactSpeed, minImpactSpeed)
            ? heavyImpactSound
            : lightImpactSound;

        if (string.IsNullOrEmpty(sound)) return;

        audio.Play(sound);
        _nextSoundTime = Time.time + cooldown;
    }
}
