using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 1.5f)] public float pitch = 1f;
        public bool loop = false;
    }

    [SerializeField] private Sound[] sounds;

    private AudioSource musicSource;
    private readonly List<AudioSource> sfxPool = new List<AudioSource>();
    private const int SFX_POOL_SIZE = 8;

    private Dictionary<string, Sound> lookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        lookup = new Dictionary<string, Sound>();
        foreach (var s in sounds)
        {
            if (string.IsNullOrEmpty(s.name)) continue;
            if (lookup.ContainsKey(s.name))
            {
                Debug.LogWarning($"Duplicate sound name: {s.name}");
                continue;
            }
            lookup.Add(s.name, s);
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;

        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sfxPool.Add(src);
        }
    }

    public void Play(string name, float volumeMultiplier = 1f)
    {
        if (!lookup.TryGetValue(name, out var sound))
        {
            Debug.LogWarning($"Sound '{name}' was not found");
            return;
        }
        if (sound.clip == null) return;

        Debug.Log($"Playing '{name}', clip={sound.clip.name}");
        var src = GetFreeSource();
        src.spatialBlend = 0f;
        src.clip = sound.clip;
        src.volume = sound.volume * volumeMultiplier;
        src.pitch = sound.pitch;
        src.loop = sound.loop;
        src.Play();
    }

    public void Stop(string name)
    {
        if (!lookup.TryGetValue(name, out var sound)) return;

        foreach (var src in sfxPool)
        {
            if (src.isPlaying && src.clip == sound.clip)
                src.Stop();
        }
        if (musicSource.isPlaying && musicSource.clip == sound.clip)
            musicSource.Stop();
    }

    public void PlayMusic(string name, float volumeMultiplier = 1f)
    {
        if (!lookup.TryGetValue(name, out var sound))
        {
            Debug.LogWarning($"Music '{name}' was not found");
            return;
        }
        if (sound.clip == null) return;

        musicSource.clip = sound.clip;
        musicSource.volume = sound.volume * volumeMultiplier;
        musicSource.pitch = sound.pitch;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    private AudioSource GetFreeSource()
    {
        foreach (var src in sfxPool)
            if (!src.isPlaying) return src;

        return sfxPool[0];
    }
}