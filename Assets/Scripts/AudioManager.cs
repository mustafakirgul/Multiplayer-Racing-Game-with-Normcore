using UnityEngine;

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
        DontDestroyOnLoad(this.gameObject);
    }
    private void Awake()
    {
        SingletonCheck();
    }
    [Range(0f,1f)]
    public float sfxVolume=.5f;
    [Range(0f,1f)]
    public float musicVolume=.5f;
    public bool sfxIsOn = true;
    public bool musicIsOn = true;
}