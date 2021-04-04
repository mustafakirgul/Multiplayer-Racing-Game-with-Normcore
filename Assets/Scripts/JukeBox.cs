using System;
using UnityEngine;

public class JukeBox : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource source;
    public State state;
    public Music[] musics;
    [Range(0f, 1f)] public float desiredVolume;
    public bool setVolume;

    void Start()
    {
        source = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        source.playOnAwake = false;
        source.loop = true;
    }

    private void Update()
    {
        if (!setVolume) return;
        if (source != null) source.volume = desiredVolume;
    }

    public void SetVolume(float value)
    {
        source.volume = Mathf.Clamp01(value);
    }

    public void SwitchState(State _state)
    {
        if (_state == state) return;
        state = _state;
        source.Stop();
        source.clip = null;
        for (int i = 0; i < musics.Length; i++)
        {
            if (musics[i].state == state)
            {
                source.clip = musics[i].music;
                source.Play();
            }
        }
    }
}

[Serializable]
public struct Music
{
    public State state;
    public AudioClip music;
}

public enum State
{
    menu,
    game,
    off
}