using System;
using System.Collections;
using Normal.Realtime;
using UnityEngine;

public class EngineSound : MonoBehaviour
{
    public AudioSource source;
    public AudioClip engineStart, engineRun, engineStop;
    [SerializeField] private float topSpeed, divider;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float pitch;
    [SerializeField] private NewCarController controller;
    [SerializeField] private bool isInitialized;
    [SerializeField] private bool isRunning, isLocal;

    private void Initialize()
    {
        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.minDistance = 1f;
        source.maxDistance = 50f;

        isLocal = transform.parent.GetComponent<RealtimeView>().isOwnedLocallyInHierarchy;
        if (isLocal)
        {
            if (controller == null) controller = transform.parent.GetComponent<NewCarController>();
            {
                if (controller != null)
                {
                    topSpeed = controller.MaxSpeed;
                }
            }
            source.spatialBlend = .75f;
            source.spread = 66f;
        }
        else
        {
            source.spatialBlend = 1f;
            source.spread = 360f;
            topSpeed = 1.25f;
        }

        isInitialized = isLocal ? controller != null && source != null : source != null;
    }

    private void Start()
    {
        Initialize();
        StartEngine();
    }

    public void StartEngine()
    {
        if (!isRunning)
        {
            StartCoroutine(CR_StartEngine());
        }
    }

    public void StopEngine()
    {
        isRunning = false;
        source.Stop();
        PlaySound(engineStop, false);
    }

    private IEnumerator CR_StartEngine()
    {
        if (engineStart != null)
        {
            PlaySound(engineStart, false);
            yield return new WaitForSeconds(engineStart.length * .9f);
        }

        PlaySound(engineRun, true);
        isRunning = true;
        yield return null;
    }

    public void PlaySound(AudioClip a, bool loop)
    {
        if (!AudioManager.instance.sfxIsOn) return;
        if (isInitialized) Initialize();
        if (loop)
        {
            source.loop = true;
            source.clip = a;
            source.Play();
        }
        else source.PlayOneShot(a);
    }

    private void Update()
    {
        if (isRunning) MakeSound();
    }

    void MakeSound()
    {
        currentSpeed = isLocal ? controller.CarRB.velocity.magnitude : 1f;
        pitch = currentSpeed / (topSpeed / divider);
        source.pitch = Mathf.Clamp(pitch, .25f, 1f);
    }
}