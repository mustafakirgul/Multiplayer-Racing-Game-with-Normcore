using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private MenuController menuController;

    public void RegisterMenuController(MenuController _menuController)
    {
        menuController = _menuController;
    }

    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    private void Awake()
    {
        SingletonCheck();
    }

    [Range(0f, 1f)] public float sfxVolume = .5f;
    [Range(0f, 1f)] public float musicVolume = .5f;
    [Range(0f, 1f)] public float masterVolume = .5f;
    public bool sfxIsOn = true;
    public bool musicIsOn = true;
    public Slider sfx, music, master;
    public Image toggleMusic;
    public Image toggleSFX;
    public Sprite onImage;
    public Sprite offImage;
    public JukeBox jukeBox;
    private bool menuState;

    private void Start()
    {
        jukeBox = FindObjectOfType<JukeBox>();
        if (PlayerPrefs.HasKey("masterV"))
        {
            masterVolume = PlayerPrefs.GetFloat("masterV");
            master.value = masterVolume;
        }

        if (PlayerPrefs.HasKey("musicV"))
        {
            musicVolume = PlayerPrefs.GetFloat("musicV");
            music.value = musicVolume;
        }

        if (PlayerPrefs.HasKey("SFXV"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SFXV");
            sfx.value = sfxVolume;
        }

        if (PlayerPrefs.HasKey("musicIsOn"))
        {
            musicIsOn = PlayerPrefs.GetInt("musicIsOn") == 1;
            toggleMusic.sprite = musicIsOn ? onImage : offImage;
        }

        if (PlayerPrefs.HasKey("sfxIsOn"))
        {
            sfxIsOn = PlayerPrefs.GetInt("sfxIsOn") == 1;
            toggleSFX.sprite = sfxIsOn ? onImage : offImage;
        }

        UpdateEngineSoundLevels();
    }

    void UpdateEngineSoundLevels()
    {
        var engines = FindObjectsOfType<EngineSound>();
        for (int i = 0; i < engines.Length; i++)
        {
            engines[i].source.volume = sfxVolume;
            if (!sfxIsOn)
                engines[i].StopEngine();
            else
                engines[i].StartEngine();
        }
    }

    public void UpdateMusicVolume(float value)
    {
        musicVolume = value * masterVolume;
        jukeBox.SetVolume(musicVolume);
        PlayerPrefs.SetFloat("musicV", musicVolume);
    }

    public void UpdateSFXVolume(float value)
    {
        sfxVolume = value * masterVolume;
        UpdateEngineSoundLevels();
        PlayerPrefs.SetFloat("SFXV", sfxVolume);
    }

    public void UpdateMasterVolume(float value)
    {
        masterVolume = value;
        UpdateSFXVolume(sfx.value);
        UpdateMusicVolume(music.value);
        PlayerPrefs.SetFloat("masterV", masterVolume);
    }

    public void ToggleMusic()
    {
        musicIsOn = !musicIsOn;
        toggleMusic.sprite = musicIsOn ? onImage : offImage;
        if (musicIsOn) jukeBox.SwitchState(State.menu);
        else jukeBox.SwitchState(State.off);
        PlayerPrefs.SetInt("musicIsOn", musicIsOn ? 1 : 0);
    }

    public void ToggleSFX()
    {
        sfxIsOn = !sfxIsOn;
        GameManager.instance.AmbientSound(sfxIsOn);
        toggleSFX.sprite = sfxIsOn ? onImage : offImage;
        PlayerPrefs.SetInt("sfxIsOn", sfxIsOn ? 1 : 0);
        UpdateEngineSoundLevels();
        var truck = FindObjectOfType<Truck>();
        if (truck == null) return;
        if (sfxIsOn)
            truck.transform.GetChild(0).GetComponent<AudioSource>().Play();
        else
            truck.transform.GetChild(0).GetComponent<AudioSource>().Stop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.instance._race.m_isOn)
        {
            if (menuController != null)
            {
                menuState = !menuState;
                if (menuState) menuController.ShowOptions();
                else menuController.HideOptions();
            }
        }
    }
}