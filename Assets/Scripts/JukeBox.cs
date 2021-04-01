using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JukeBox : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource player;
    public State state;
    public Music[] musics;
    [Range(0f, 1f)] public float desiredVolume;
    public bool setVolume;

    void Start()
    {
        player = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        player.playOnAwake = false;
        player.loop = true;
    }

    private void Update()
    {
        if (setVolume)
        {
            if (player != null)
            {
                player.volume = desiredVolume;
                setVolume = false;
            }
        }
    }

    public void SwitchState(State _state)
    {
        if (_state == state) return;
        state = _state;
        player.Stop();
        player.clip = null;
        for (int i = 0; i < musics.Length; i++)
        {
            if (musics[i].state == state)
            {
                player.clip = musics[i].music;
                player.Play();
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