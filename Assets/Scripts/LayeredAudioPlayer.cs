using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using Random = UnityEngine.Random;

public class LayeredAudioPlayer : MonoBehaviour
{
    private AudioSource source;
    public AudioLayer[] layers;
    public bool playAtStart;
    public bool randomizeAtPlay;
    [Range(0f, 1f)] public float spatialBlend = 0.666f;
    [Range(0f, 360f)] public float spread = 0f;

    private void Initialize()
    {
        if (layers == null) return;
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
        Play();
    }

    public void Play()
    {
        if (AudioManager.instance.sfxIsOn)
        {
            if (layers != null)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    var index = 0;
                    if (randomizeAtPlay) index = Random.Range(0, layers[i].audioClips.Length);
                    source.volume = AudioManager.instance.sfxVolume;
                    source.PlayOneShot(layers[i].audioClips[index]);
                }
            }
        }
    }
}

[Serializable]
public struct AudioLayer
{
    public string definition;
    public AudioClip[] audioClips;
}