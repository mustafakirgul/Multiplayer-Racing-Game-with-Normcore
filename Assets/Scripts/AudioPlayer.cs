using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    AudioSource source => GetComponent<AudioSource>();
    public AudioClip[] sounds;
    public bool debug;

    private void Start()
    {
        if (debug) PlaySound(0);
    }

    public void PlaySound(int index)
    {
        if (sounds != null)
        {
            if (index <= sounds.Length)
            {
                if (sounds[index] != null)
                {
                    source.clip = sounds[index];
                    source.PlayOneShot(sounds[index]);
                    return;
                }
            }
        }

        Debug.LogWarning("Sound " + index + " for " + transform.name + " has no audio clip attached!");
    }
}