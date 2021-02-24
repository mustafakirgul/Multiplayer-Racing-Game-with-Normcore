using System;
using System.Text;
using Normal.Realtime;
using UnityEngine;

public class StatsEntity : RealtimeComponent<StatsModel>
{
    [Header("Stats")] public int _kills;
    public float _damageToTruck;
    public int _powerUp;
    public int _loot;
    [Header("Players&Stats")] public StatsEntity[] entities;
    [Space(10)] public StatsEntry[] stats;
    StringBuilder sb = new StringBuilder("", 666);

    protected override void OnRealtimeModelReplaced(StatsModel previousModel, StatsModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);
        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                currentModel.kills = _kills;
                currentModel.loot = _loot;
                currentModel.powerUp = _powerUp;
                currentModel.damageToTruck = _damageToTruck;
                return;
            }

            _kills = currentModel.kills;
            _damageToTruck = currentModel.damageToTruck;
            _powerUp = currentModel.powerUp;
            _loot = currentModel.loot;
        }
    }

    private void Start()
    {
        if (realtimeView.isOwnedLocallyInHierarchy)
            PlayerManager.instance.statsEntity = this;
    }

    public StatsEntry[] ReturnStats()
    {
        entities = FindObjectsOfType<StatsEntity>();
        var length = entities.Length;
        stats = new StatsEntry[length];
        for (int i = 0; i < length; i++)
        {
            var entity = entities[i];
            stats[i] = new StatsEntry(
                PlayerManager.instance.PlayerName(entity.gameObject.GetComponent<RealtimeView>().ownerIDInHierarchy),
                entity._kills, entity._damageToTruck, entity._powerUp, entity._loot);
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


    public void SendStat(StatType type, float value)
    {
        switch (type)
        {
            case StatType.damage:
                _damageToTruck += value;
                break;
            default:
                Debug.LogError(
                    "Please do not send a float with any statType other than 'damageToTruck' as all others increment by 1.");
                break;
        }
    }

    public void SendStat(StatType type)
    {
        switch (type)
        {
            case StatType.kill:
                _kills++;
                break;
            case StatType.damage:
                Debug.LogError(
                    "Please send a float with statType 'damageToTruck' so that I know how much damage you did to the truck!");
                break;
            case StatType.powerup:
                _powerUp++;
                break;
            case StatType.loot:
                _loot++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
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

    public string playerName;
    public int kills;
    public float damageToTruck;
    public int powerUp;
    public int loot;
}