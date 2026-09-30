using UnityEngine;
public class test : MonoBehaviour
{
    public AudioClip clip;
    void Start()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        src.clip = clip;
        src.Play();
        Debug.Log($"test isPlaying={src.isPlaying}");
    }
}