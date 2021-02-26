using System;
using System.Collections;
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

    public float checkInterval;
    [Header("Players&Stats")] public StatsEntity[] entities;
    [Space(10)] public StatsEntry[] stats;
    StringBuilder sb = new StringBuilder("", 666);
    private Coroutine entityChecker;
    private WaitForSeconds wait;

    private void Awake()
    {
        SingletonCheck();
    }
    private void Start()
    {
        wait = new WaitForSeconds(checkInterval);
        if (entityChecker != null) StopCoroutine(entityChecker);
        entityChecker = StartCoroutine(CR_CheckStats());
    }

    public StatsEntry[] ReturnStats()
    {
        var length = entities.Length;
        stats = new StatsEntry[length];
        for (int i = 0; i < length; i++)
        {
            var entity = entities[i];
            stats[i] = new StatsEntry(entity);
        }

        return stats;
    }

    public string ReturnStringStats()
    {
        sb.Clear();
        var _stats = ReturnStats();
        var length = _stats.Length;
        for (int i = 0; i < length; i++)
        {
            sb.Append(_stats[i].playerName + " has killed " + _stats[i].kills + " rivals, " + "did " +
                      _stats[i].damageToTruck + " damage to the truck, collected " + _stats[i].powerUp +
                      " powerup(s) and " + _stats[i].loot + " loot container(s).");
            if (i < length - 1)
            {
                sb.Append("\n");
            }
        }

        return sb.ToString();
    }

    IEnumerator CR_CheckStats()
    {
        while (entityChecker != null)
        {
            entities = FindObjectsOfType<StatsEntity>();
            var length = entities.Length;
            stats = new StatsEntry[length];
            yield return wait;
        }
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