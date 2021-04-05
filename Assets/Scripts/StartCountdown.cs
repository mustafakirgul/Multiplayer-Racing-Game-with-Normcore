using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StartCountdown : MonoBehaviour
{
    private Race race;
    public int start;

    [Tooltip(
        "if this is true, the number of countdown is bound to the number of lights on the countdown lights gadget")]
    public bool autoStartCount = true;

    public int current;
    private bool isRunning;
    private Coroutine cr_Countdown;
    private WaitForSeconds waitASec;
    private AudioPlayer audioPlayer;
    public UnityEvent endEvent;
    public CountdownLights countdownLights;

    private void Start()
    {
        current = start;
    }

    public void Initialize(Race _race)
    {
        audioPlayer = GetComponent<AudioPlayer>();
        waitASec = new WaitForSeconds(1f);
        if (cr_Countdown != null) StopCoroutine(cr_Countdown);
        race = _race;
        if (autoStartCount)
            if (countdownLights != null)
                countdownLights = FindObjectOfType<CountdownLights>();
        start = countdownLights.lights.Length;
        current = start;
        cr_Countdown = StartCoroutine(CR_Countdown());
    }

    IEnumerator CR_Countdown()
    {
        while (current > 0)
        {
            current = race.CountOneDown();
            countdownLights.TurnOntheLight(current);
            audioPlayer.Play();
            yield return waitASec;
        }

        endEvent.Invoke();
        yield return null;
    }
}