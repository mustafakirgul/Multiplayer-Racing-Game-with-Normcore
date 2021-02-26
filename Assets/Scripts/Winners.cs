using System;
using UnityEngine;
using UnityEngine.UI;

public class Winners : MonoBehaviour
{
    private Text winner, mostCars, mostDamage, mostLoot;
    private StatsEntry[] results;
    private string playerName;

    private void Awake()
    {
        winner = transform.GetChild(1).GetChild(0).GetComponent<Text>();
        mostCars = transform.GetChild(1).GetChild(1).GetComponent<Text>();
        mostDamage = transform.GetChild(1).GetChild(2).GetComponent<Text>();
        mostLoot = transform.GetChild(1).GetChild(3).GetComponent<Text>();
    }

    private void OnEnable()
    {
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
            if (results[i].loot > highest)
            {
                highest = results[i].loot;
                playerName = results[i].playerName;
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
        return "PLATYPUS";
    }

    private void UpdateMostDamage()
    {
        if (mostDamage == null) return;
        mostDamage.text = ReturnMostDamage();
    }

    void RetrieveResults()
    {
        if (StatsManager.instance == null) return;
        results = StatsManager.instance.ReturnStats();
    }

    private string ReturnMostDamage()
    {
        playerName = "";
        var highest = -0.1f;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i].damageToTruck > highest)
            {
                highest = results[i].damageToTruck;
                playerName = results[i].playerName;
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
            if (results[i].kills > highest)
            {
                highest = results[i].kills;
                playerName = results[i].playerName;
            }
        }

        return playerName;
    }
}