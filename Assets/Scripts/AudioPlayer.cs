using System;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    AudioSource source;
    public AudioClip[] sounds;
    public bool playAtStart;

    private void OnValidate()
    {
        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (playAtStart) PlaySound(0);
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