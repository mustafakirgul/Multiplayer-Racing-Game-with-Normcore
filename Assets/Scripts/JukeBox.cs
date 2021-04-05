using System;
using UnityEngine;

public class JukeBox : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource source;
    public State state;
    public Music[] musics;
    [Range(0f, 1f)] public float desiredVolume;
    public bool setVolume;

    void Start()
    {
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
        if (!AudioManager.instance.musicIsOn && _state != State.off) return;
        state = _state;
        source.Stop();
        source.clip = null;
        if (state == State.off) return;
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