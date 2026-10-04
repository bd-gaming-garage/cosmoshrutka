using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MarshrutkaController))]
public class MarshrutkaEngineAudio : MonoBehaviour
{
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip lowRpmClip;
    [SerializeField] private AudioClip highRpmClip;

    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField, Min(0.1f)] private float fullRpmSpeed = 25f;
    [SerializeField, Min(0.1f)] private float response = 2f;
    [SerializeField, Range(0f, 1f)] private float throttleContribution = 0.3f;

    private MarshrutkaController _bus;
    private AudioSource _idle;
    private AudioSource _low;
    private AudioSource _high;
    private float _rpm;

    private void Awake()
    {
        _bus = GetComponent<MarshrutkaController>();
        _idle = CreateSource(idleClip);
        _low = CreateSource(lowRpmClip);
        _high = CreateSource(highRpmClip);
    }

    private AudioSource CreateSource(AudioClip clip)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        source.clip = clip;
        return source;
    }

    private void OnEnable()
    {
        _rpm = 0f;
        ApplyMix();
        SetPlaying(_bus.isActiveAndEnabled);
    }

    private void LateUpdate()
    {
        if (!_bus.isActiveAndEnabled)
        {
            SetPlaying(false);
            _rpm = 0f;
            return;
        }

        float speed = Mathf.Clamp01(
            Mathf.Abs(_bus.SignedSpeed) / Mathf.Max(fullRpmSpeed, 0.1f));
        float target = Mathf.Clamp01(
            speed + _bus.Throttle * throttleContribution);

        _rpm = Mathf.MoveTowards(_rpm, target, response * Time.deltaTime);
        ApplyMix();
        SetPlaying(true);
    }

    private void ApplyMix()
    {
        float idleWeight = 1f - Mathf.Clamp01(_rpm * 2f);
        float highWeight = Mathf.Clamp01(_rpm * 2f - 1f);
        float lowWeight = 1f - idleWeight - highWeight;

        _idle.volume = volume * idleWeight;
        _low.volume = volume * lowWeight;
        _high.volume = volume * highWeight;
    }

    private void SetPlaying(bool playing)
    {
        SetPlaying(_idle, playing);
        SetPlaying(_low, playing);
        SetPlaying(_high, playing);
    }

    private static void SetPlaying(AudioSource source, bool playing)
    {
        if (playing)
        {
            if (source.clip != null && !source.isPlaying)
                source.Play();
        }
        else
        {
            source.Stop();
        }
    }

    private void OnDisable()
    {
        SetPlaying(false);
    }
}
