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

        //return unordered results
        if (StatsManager.instance == null) return;
        results = StatsManager.instance.ReturnStats();
        var length = results.Length;
        //clear ordered list to use it as a buffer before ordering
        if (orderedResults == null)
            orderedResults = new List<OrderedEntry>();
        else
            orderedResults.Clear();
        //copy everything to ordered list (without ordering, yet)
        for (int i = 0; i < length; i++)
        {
            //TODO calculate score better
            //calculate score - it is the total of all numbers in a stats entity for now
            int score = results[i].kills + Convert.ToInt32(results[i].damageToTruck) + results[i].loot +
                        results[i].powerUp;
            orderedResults.Add(new OrderedEntry(results[i], score));
        }

        //sort the ordered list by score to make it ordered for real!
        orderedResults.Sort(SortByScore);

        //convert information to string to show it on the game screen live!
        sb.Clear();
        for (int i = 0; i < length; i++)
        {
            sb.Append((i + 1) + ". " + orderedResults[i].result.playerName);
            sb.Append(" / KILLS " + orderedResults[i].result.kills);
            sb.Append(" / DMG " + orderedResults[i].result.damageToTruck);
            sb.Append(" / PWU " + orderedResults[i].result.powerUp);
            sb.Append(" / LOOT " + orderedResults[i].result.loot);
            sb.Append(" / SCORE " + (orderedResults[i].result.kills + orderedResults[i].result.damageToTruck +
                                     orderedResults[i].result.powerUp + orderedResults[i].result.loot));

            if (i < length - 1)
            {
                sb.Append("\n");
            }
        }

        display.text = sb.ToString();
    }

    private int SortByScore(OrderedEntry p1, OrderedEntry p2)
    {
        return p2.score.CompareTo(p1.score);
    }
}