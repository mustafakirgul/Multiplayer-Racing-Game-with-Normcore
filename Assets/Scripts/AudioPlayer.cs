using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    AudioSource source;
    public AudioClip[] sounds;
    public bool playAtStart;
    public bool randomizeAtPlay;

    private void OnValidate()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = .666f;
        source.spread = 180;
        source.minDistance = 1f;
        source.maxDistance = 500f;
    }

    private void Start()
    {
        Initialize();
        if (!playAtStart) return;
        if (randomizeAtPlay) PlaySound(Random.Range(0, sounds.Length));
        else PlaySound(0);
    }

    public void PlaySound(int index)
    {
        if (source == null) Initialize();
        if (sounds != null)
        {
            if (index <= sounds.Length)
            {
                if (sounds[index] != null)
                {
                    if (source.isPlaying) source.Stop();
                    source.clip = sounds[index];
                    source.Play();
                    return;
                }
            }
        }

        Debug.LogWarning("Sound " + index + " for " + transform.name + " has no audio clip attached!");
    }
}