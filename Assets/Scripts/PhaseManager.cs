using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Normal.Realtime;

public class PhaseManager : MonoBehaviour
{
    public List<Phase> phases;
    public int phase;
    public TextMeshProUGUI messageTMPro;
    public UnityEvent endOfPhasesEvent;
    private Coroutine timer, displayMessage;
    private WaitForSeconds wait, waitForMessage;

    [SerializeField]
    private GameObject[] Walls;

    public void StartPhaseSystem()
    {
        JumpToPhase(0);
    }

    public void NextPhase() //called locally by the local game manager, depending on conditions
    {
        phase++;
        if (phase == phases.Count)
            endOfPhasesEvent.Invoke();
        //phase %= phases.Count;
        if (phase <= phases.Count - 1)
            GameManager.instance._race.ChangePhase(phase);
    }

    public void JumpToPhase(int newPhase) //called by the network instance if the phase number is changed
    {
        if (newPhase != -1) // use -1 as phase index to initialize it
            newPhase = Mathf.Abs(newPhase);
        phase = newPhase;
        phases[phase].startEvent.Invoke();
        if (phases[phase].startMessage.Length > 0)
        {
            if (displayMessage != null) StopCoroutine(displayMessage);
            displayMessage = StartCoroutine(CR_DisplayMessage(phases[phase].startMessage));
        }

        if (phases[phase].type == PhaseType.timeBased
        ) //if time based set up the timer - if not, just wait for someone to trigger the transition
            StartTimer(phases[phase].duration);
    }

    private void StartTimer(float duration)
    {
        wait = new WaitForSeconds(duration);
        if (timer != null) StopCoroutine(timer);
        timer = StartCoroutine(CR_Timer());
    }

    IEnumerator CR_Timer()
    {
        yield return wait;
        NextPhase();
    }

    IEnumerator CR_DisplayMessage(string message)
    {
        waitForMessage = new WaitForSeconds(message.Length * 0.1f);
        messageTMPro.SetText(message);
        yield return waitForMessage;
        messageTMPro.SetText("");
        yield return null;
    }
}

[Serializable]
public struct Phase
{
    public string startMessage;
    public PhaseType type;
    [Tooltip("If phase is time based")] public float duration; //if timed phase

    [Tooltip("Triggers when phase starts")]
    public UnityEvent startEvent;
}

public enum PhaseType
{
    timeBased,
    conditionBased
}