using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        transform.parent = null;
    }

    private void Awake()
    {
        SingletonCheck();
    }

    [Range(0f, 1f)] public float sfxVolume = .5f;
    [Range(0f, 1f)] public float musicVolume = .5f;
    public bool sfxIsOn = true;
    public bool musicIsOn = true;

    public Image toggleMusic;
    public Image toggleSFX;
    public Sprite onImage;
    public Sprite offImage;
    public Slider masterSlider, musicSlider, SFXSlider;
    public JukeBox jukeBox;

    public void UpdateMusicVolume(float value)
    {
        musicVolume = value * masterSlider.value;
        jukeBox.SetVolume(musicVolume);
    }

    public void UpdateSFXVolume(float value)
    {
        sfxVolume = value * masterSlider.value;
    }

    public void ToggleMusic()
    {
        musicIsOn = !musicIsOn;
        toggleMusic.sprite = musicIsOn ? onImage : offImage;
        if (musicIsOn) jukeBox.SwitchState(State.menu);
        else jukeBox.SwitchState(State.off);
    }

    public void ToggleSFX()
    {
        sfxIsOn = !sfxIsOn;
        toggleSFX.sprite = sfxIsOn ? onImage : offImage;
    }
}