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
//return ordered results
        if (StatsManager.instance == null) return;
        if (orderedResults == null) orderedResults = new List<OrderedEntry>();
        orderedResults.AddRange(StatsManager.instance.ReturnOrderedStats());

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