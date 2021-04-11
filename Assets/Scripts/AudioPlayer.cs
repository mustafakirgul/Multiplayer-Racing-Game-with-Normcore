using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public string identifier; //cosmetic
    AudioSource source;
    public AudioClip[] sounds;
    public bool playAtStart;
    public bool randomizeAtStart;
    [Range(0f, 1f)] public float spatialBlend = 0.666f;
    [Range(0f, 360f)] public float spread = 0f;

    private void Initialize()
    {
        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = spatialBlend;
        source.spread = spread;
        source.minDistance = 1f;
        source.maxDistance = 500f;
    }

    private void Start()
    {
        Initialize();
        if (!playAtStart) return;
        if (randomizeAtStart) PlayRandom();
        else PlayIndex(0);
    }

    public void PlayRandom()
    {
        PlayIndex(Random.Range(0, sounds.Length));
    }

    public void Play()
    {
        PlayIndex(0);
    }

    public void PlayIndex(int index)
    {
        if (AudioManager.instance.sfxIsOn)
        {
            if (source == null) Initialize();
            if (sounds != null)
            {
                if (index < sounds.Length)
                {
                    if (sounds[index] != null)
                    {
                        source.volume = AudioManager.instance.sfxVolume;
                        source.PlayOneShot(sounds[index]);
                    }
                }
            }
        }
    }
}