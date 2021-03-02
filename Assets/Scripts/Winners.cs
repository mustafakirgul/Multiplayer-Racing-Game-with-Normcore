using System;
using UnityEngine;
using UnityEngine.UI;

public class Winners : MonoBehaviour
{
    private Text winner, mostCars, mostDamage, mostLoot;
    private OrderedEntry[] results;
    private string playerName;

    private void Awake()
    {
        mostLoot = transform.GetChild(1).GetChild(3).GetComponent<Text>();
        winner = transform.GetChild(1).GetChild(0).GetComponent<Text>();
        mostCars = transform.GetChild(1).GetChild(1).GetComponent<Text>();
        mostDamage = transform.GetChild(1).GetChild(2).GetComponent<Text>();
    }

    private void OnEnable()
    {
        FindObjectOfType<TopRacersLive>().isRunning = false;
        RetrieveResults();
        if (results == null) return;
        UpdateWinner();
        UpdateMostCars();
        UpdateMostDamage();
        UpdateMostLoot();
    }

    private void UpdateMostLoot()
    {
        if (mostLoot == null) return;
        mostLoot.text = ReturnMostLoot();
    }

    private string ReturnMostLoot()
    {
        playerName = "";
        var highest = -1;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i].result.loot > highest)
            {
                highest = results[i].result.loot;
                playerName = results[i].result.playerName;
            }
        }

        return playerName;
    }

    private void UpdateWinner()
    {
        if (winner == null) return;
        winner.text = CalculateWinner();
    }

    private string CalculateWinner()
    {
        playerName = "";
        var highest = -1;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i].score > highest)
            {
                highest = results[i].score;
                playerName = results[i].result.playerName;
            }
        }

        return playerName;
    }

    private void UpdateMostDamage()
    {
        if (mostDamage == null) return;
        mostDamage.text = ReturnMostDamage();
    }

    void RetrieveResults()
    {
        if (StatsManager.instance == null) return;
        results = StatsManager.instance.ReturnOrderedStats();
    }

    private string ReturnMostDamage()
    {
        playerName = "";
        var highest = -0.1f;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i].result.damageToTruck > highest)
            {
                highest = results[i].result.damageToTruck;
                playerName = results[i].result.playerName;
            }
        }

        return playerName;
    }

    void UpdateMostCars()
    {
        if (mostCars == null) return;
        mostCars.text = ReturnMostCarsDestroyed();
    }

    string ReturnMostCarsDestroyed()
    {
        playerName = "";
        var highest = -1;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i].result.kills > highest)
            {
                highest = results[i].result.kills;
                playerName = results[i].result.playerName;
            }
        }

        return playerName;
    }
}