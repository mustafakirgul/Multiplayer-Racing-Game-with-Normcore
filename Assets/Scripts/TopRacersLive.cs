using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TopRacersLive : MonoBehaviour
{
    public float interval;
    public Text display;
    private float _interval;
    private Coroutine cr_Check;
    private WaitForSeconds wait;
    public bool isRunning;
    private StatsEntry[] results;
    private List<OrderedEntry> orderedResults;
    [Space(10)] public StatsEntry[] stats;
    StringBuilder sb = new StringBuilder("", 666);

    private void Start()
    {
        if (interval <= 0)
            interval = 1f;
        if (display == null)
            display = GetComponent<Text>();
        _interval = interval;
        wait = new WaitForSeconds(_interval);
        cr_Check = StartCoroutine(CR_Check());
    }

    private IEnumerator CR_Check()
    {
        while (true)
        {
            while (isRunning)
            {
                if (_interval != interval)
                {
                    _interval = interval;
                    wait = new WaitForSeconds(_interval);
                }

                GetResults();
                yield return wait;
            }

            yield return wait;
        }
    }

    private void GetResults()
    {
        if (display == null)
        {
            Debug.LogWarning("No display to show top racers!");
            return;
        }

        //return ordered results
        if (StatsManager.instance == null) return;
        if (orderedResults == null)
            orderedResults = new List<OrderedEntry>();
        else
            orderedResults.Clear();
        orderedResults.AddRange(StatsManager.instance.ReturnOrderedStats());
        var length = orderedResults.Count;
        //convert information to string to show it on the game screen live!
        sb.Clear();
        sb.Append("RACER_____SCORE");
        for (int i = 0; i < length; i++)
        {
            sb.Append((i + 1) + ". " + orderedResults[i].result.playerName);
            for (int j = 1; j == 11 - orderedResults[i].result.playerName.Length; j++)
            {
                sb.Append(" ");
            }

            sb.Append(" / SCORE " + orderedResults[i].score);

            if (i < length - 1)
            {
                sb.Append("\n");
            }
        }

        display.text = sb.ToString();
    }
}