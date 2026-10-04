using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MarshrutkaController))]
public class MarshrutkaEngineAudio : MonoBehaviour
{
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip lowRpmClip;
    [SerializeField] private AudioClip highRpmClip;

    [SerializeField, Range(0f, 1f)] private float volume = 0.22f;
    [SerializeField, Min(0.1f)] private float fullRpmSpeed = 25f;
    [SerializeField, Min(0.1f)] private float response = 2f;
    [SerializeField, Range(0f, 1f)] private float throttleContribution = 0.3f;

    [Tooltip("Overlap between the end and start of each engine recording, in seconds")]
    [SerializeField, Min(0.01f)] private float loopCrossfade = 0.15f;

    private MarshrutkaController _bus;
    private CrossfadeLoop _idle;
    private CrossfadeLoop _low;
    private CrossfadeLoop _high;
    private float _rpm;

    private void Awake()
    {
        _bus = GetComponent<MarshrutkaController>();
        _idle = new CrossfadeLoop(gameObject, idleClip, loopCrossfade);
        _low = new CrossfadeLoop(gameObject, lowRpmClip, loopCrossfade);
        _high = new CrossfadeLoop(gameObject, highRpmClip, loopCrossfade);
    }

    private void LateUpdate()
    {
        if (!_bus.isActiveAndEnabled)
        {
            StopLoops();
            _rpm = 0f;
            return;
        }

        float speed = Mathf.Clamp01(
            Mathf.Abs(_bus.SignedSpeed) / Mathf.Max(fullRpmSpeed, 0.1f));
        float target = Mathf.Clamp01(
            speed + _bus.Throttle * throttleContribution);

        _rpm = Mathf.MoveTowards(_rpm, target, response * Time.deltaTime);
        ApplyMix();
    }

    private void ApplyMix()
    {
        float idleWeight = 1f - Mathf.Clamp01(_rpm * 2f);
        float highWeight = Mathf.Clamp01(_rpm * 2f - 1f);
        float lowWeight = 1f - idleWeight - highWeight;

        double now = AudioSettings.dspTime;
        _idle.Update(now, volume * idleWeight);
        _low.Update(now, volume * lowWeight);
        _high.Update(now, volume * highWeight);
    }

    private void StopLoops()
    {
        _idle.Stop();
        _low.Stop();
        _high.Stop();
    }

    private void OnDisable()
    {
        StopLoops();
        _rpm = 0f;
    }

    private sealed class CrossfadeLoop
    {
        private const double ScheduleLead = 0.1;
        private readonly AudioClip _clip;
        private readonly AudioSource[] _sources = new AudioSource[2];
        private readonly double[] _starts = new double[2];
        private readonly double _duration;
        private readonly double _overlap;
        private bool _running;

        public CrossfadeLoop(GameObject owner, AudioClip clip, float crossfade)
        {
            _clip = clip;
            if (clip != null)
            {
                _duration = (double)clip.samples / clip.frequency;
                // Keep enough time to reuse a source after its previous playback ends.
                _overlap = System.Math.Min(System.Math.Max(crossfade, 0.01), _duration * 0.25);
                if (clip.loadState == AudioDataLoadState.Unloaded)
                    clip.LoadAudioData();
            }

            for (int i = 0; i < _sources.Length; i++)
            {
                var source = owner.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                source.volume = 0f;
                source.clip = clip;
                _sources[i] = source;
            }
        }

        public void Update(double now, float gain)
        {
            if (_clip == null || _duration <= 0 || _clip.loadState != AudioDataLoadState.Loaded)
                return;

            if (!_running)
                Restart(now);

            for (int i = 0; i < _sources.Length; i++)
            {
                if (now < _starts[i] + _duration) continue;

                double nextStart = _starts[1 - i] + _duration - _overlap;
                if (nextStart <= now)
                {
                    // Recover after a long frame stall without scheduling in the past.
                    Restart(now);
                    break;
                }

                Schedule(i, nextStart);
            }

            for (int i = 0; i < _sources.Length; i++)
            {
                double elapsed = now - _starts[i];
                float fadeIn = Mathf.Clamp01((float)(elapsed / _overlap));
                float fadeOut = Mathf.Clamp01((float)((_duration - elapsed) / _overlap));
                _sources[i].volume = gain * Mathf.Min(fadeIn, fadeOut);
            }
        }

        private void Restart(double now)
        {
            Stop();
            Schedule(0, now + ScheduleLead);
            Schedule(1, now + ScheduleLead + _duration - _overlap);
            _running = true;
        }

        private void Schedule(int index, double start)
        {
            var source = _sources[index];
            source.Stop();
            source.volume = 0f;
            source.timeSamples = 0;
            _starts[index] = start;
            source.PlayScheduled(start);
            source.SetScheduledEndTime(start + _duration);
        }

        public void Stop()
        {
            if (!_running) return;

            foreach (var source in _sources)
            {
                source.Stop();
                source.volume = 0f;
            }
            _running = false;
        }
    }
}
