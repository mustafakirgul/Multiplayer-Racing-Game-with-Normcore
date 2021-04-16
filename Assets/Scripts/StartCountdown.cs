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
    public CountdownLights countdownLights;
    public NewCarController localController;
    public UnityEvent endEvent;
    private UIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        current = start;
    }

    public void Initialize(Race _race)
    {
        audioPlayer = GetComponent<AudioPlayer>();
        waitASec = new WaitForSeconds(1f);
        if (cr_Countdown != null) StopCoroutine(cr_Countdown);
        race = _race;
        countdownLights = FindObjectOfType<CountdownLights>();
        if (autoStartCount && countdownLights != null)
            start = countdownLights.lights.Length;
        current = start;
        cr_Countdown = StartCoroutine(CR_Countdown());
    }

    IEnumerator CR_Countdown()
    {
        while (current > 0)
        {
            if (race == null) race = FindObjectOfType<Race>();
            current = race.CountOneDown();
            countdownLights.TurnOntheLight(current);
            if (current < start)
            {
                audioPlayer.PlayIndex(0);
            }

            yield return waitASec;
        }

        StatsManager.instance.RefreshStatEntities();
        localController.ToggleController(true);
        uiManager.EnableUI();
        FindObjectOfType<TopRacersLive>().Status(true);
        endEvent.Invoke();
        audioPlayer.PlayIndex(1);
        yield return null;
    }
}