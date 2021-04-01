using UnityEngine;

public class LayeredAudioPlayer : MonoBehaviour
{
    public AudioSource[] sources;
    public SoundPack[] soundPacks;
    public bool playAtStart;
    public bool randomizeAtPlay;

    private void OnValidate()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (soundPacks == null) return;
        sources = new AudioSource[soundPacks.Length];
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].spatialBlend = .666f;
            sources[i].spread = 90;
            sources[i].minDistance = 1f;
            sources[i].maxDistance = 500f;
        }
    }

    private void Start()
    {
        Initialize();
        if (!playAtStart) return;
        PlaySound();
    }

    public void PlaySound()
    {
        if (sources == null) Initialize();
        if (soundPacks != null)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                var index = 0;
                if (randomizeAtPlay) index = Random.Range(0, soundPacks[i].sounds.Length);

                if (sources[i].isPlaying) sources[i].Stop();
                sources[i].clip = soundPacks[i].sounds[index];
                sources[i].Play();
                return;
            }
        }
    }
}

public struct SoundPack
{
    public string definition;
    public AudioClip[] sounds;
}