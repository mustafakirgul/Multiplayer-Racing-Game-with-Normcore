using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Normal.Realtime;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    #region Singleton Logic

    public static StatsManager instance = null;

    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
    }

    #endregion

    [Header("Players&Stats")] public StatsEntity[] entities;
    [Space(10)] public StatsEntry[] stats;
    StringBuilder sb = new StringBuilder("", 666);
    private List<OrderedEntry> orderedResults;

    private void Awake()
    {
        SingletonCheck();
    }

    public OrderedEntry[] ReturnOrderedStats()
    {
        entities = FindObjectsOfType<StatsEntity>();
        var length = entities.Length;
        stats = new StatsEntry[length];
        for (int i = 0; i < length; i++)
        {
            var entity = entities[i];
            stats[i] = new StatsEntry(entity);
        }

        //clear ordered list to use it as a buffer before ordering
        if (orderedResults == null)
            orderedResults = new List<OrderedEntry>();
        else
            orderedResults.Clear();
        //copy everything to ordered list (without ordering, yet)
        for (int i = 0; i < length; i++)
        {
            //calculate score
            int score = (stats[i].kills * 100) + (Convert.ToInt32(stats[i].damageToTruck) * 3) + (stats[i].loot * 50);
            orderedResults.Add(new OrderedEntry(stats[i], score));
        }

        //sort the ordered list by score to make it ordered for real!
        orderedResults.Sort(SortByScore);
        return orderedResults.ToArray();
    }

    private int SortByScore(OrderedEntry p1, OrderedEntry p2)
    {
        return p2.score.CompareTo(p1.score);
    }
}

public enum StatType
{
    kill,
    damage,
    powerup,
    loot
}

[Serializable]
public struct StatsEntry
{
    public StatsEntry(string playerName, int kills, float damageToTruck, int powerUp, int loot)
    {
        this.playerName = playerName;
        this.kills = kills;
        this.damageToTruck = damageToTruck;
        this.powerUp = powerUp;
        this.loot = loot;
    }

    public StatsEntry(StatsEntity entity)
    {
        playerName = PlayerManager.instance.PlayerName(entity.GetComponent<RealtimeView>().ownerIDInHierarchy);
        kills = entity._kills;
        damageToTruck = entity._damageToTruck;
        powerUp = entity._powerUp;
        loot = entity._loot;
    }

    public string playerName;
    public int kills;
    public float damageToTruck;
    public int powerUp;
    public int loot;
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