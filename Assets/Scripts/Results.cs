using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Results : MonoBehaviour
{
    private StatsEntry[] results;
    private List<OrderedEntry> orderedResults;
    private ResultEntry[] entries;
    private PlayerResultLine[] lines;

    private void OnValidate()
    {
        lines = transform.GetChild(1).GetComponentsInChildren<PlayerResultLine>();
        entries = new ResultEntry[lines.Length];
        for (int i = 0; i < entries.Length; i++)
        {
            Transform _temp = lines[i].transform;
            entries[i] = new ResultEntry(
                _temp.GetChild(0).GetComponent<Text>(),
                _temp.GetChild(1).GetComponent<Text>(),
                _temp.GetChild(2).GetComponent<Text>(),
                _temp.GetChild(3).GetComponent<Text>(),
                _temp.GetChild(4).GetComponent<Text>()
            );
        }
    }

    private void OnEnable()
    {
        PopulateList();
    }

    private void PopulateList()
    {
        //return unordered results
        if (StatsManager.instance == null) return;
        results = StatsManager.instance.ReturnStats();
        //clear ordered list to use it as a buffer before ordering
        if (orderedResults == null)
            orderedResults = new List<OrderedEntry>();
        else
            orderedResults.Clear();
        //copy everything to ordered list (without ordering, yet)
        for (int i = 0; i < results.Length; i++)
        {
            //TODO calculate score better
            //calculate score - it is the total of all numbers in a stats entity for now
            int score = results[i].kills + Convert.ToInt32(results[i].damageToTruck) + results[i].loot +
                        results[i].powerUp;
            orderedResults.Add(new OrderedEntry(results[i], score));
        }

        //sort the ordered list by score to make it ordered for real!
        orderedResults.Sort(SortByScore);

        //copy information from the ordered list to UI entries;
        for (int i = 0; i < entries.Length; i++)
        {
            entries[i].playerName.text = i < orderedResults.Count ? orderedResults[i].result.playerName : "---";
            entries[i].kills.text = i < orderedResults.Count ? orderedResults[i].result.kills.ToString() : "---";
            entries[i].damageToTruck.text =
                i < orderedResults.Count
                    ? orderedResults[i].result.damageToTruck.ToString(CultureInfo.InvariantCulture)
                    : "---";
            entries[i].loot.text = i < orderedResults.Count ? orderedResults[i].result.loot.ToString() : "---";
            entries[i].score.text = i < orderedResults.Count ? orderedResults[i].score.ToString() : "---";
        }
    }

    private int SortByScore(OrderedEntry p1, OrderedEntry p2)
    {
        return p2.score.CompareTo(p1.score);
    }
}

[Serializable]
public struct ResultEntry
{
    public ResultEntry(Text playerName, Text kills, Text damageToTruck, Text loot, Text score)
    {
        this.playerName = playerName;
        this.kills = kills;
        this.damageToTruck = damageToTruck;
        this.loot = loot;
        this.score = score;
    }

    public Text playerName;
    public Text kills;
    public Text damageToTruck;
    public Text loot;
    public Text score;
}

[Serializable]
public struct OrderedEntry
{
    public OrderedEntry(StatsEntry result, int score)
    {
        this.result = result;
        this.score = score;
    }

    public StatsEntry result;
    public int score;
}