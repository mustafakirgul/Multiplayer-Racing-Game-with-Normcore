using System;
using Normal.Realtime;
using UnityEngine;

public class StatsEntity : RealtimeComponent<StatsModel>
{
    [Header("Stats")] public int _kills;
    public float _damageToTruck;
    public int _powerUp;
    public int _loot;

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
            }
        }
    }

    private void Update()
    {
        _kills = model.kills;
        _damageToTruck = model.damageToTruck;
        _powerUp = model.powerUp;
        _loot = model.loot;
    }

    public void ReceiveStat(StatType type, float value)
    {
        switch (type)
        {
            case StatType.damage:
                _damageToTruck += value;
                model.damageToTruck = _damageToTruck;
                break;
            default:
                Debug.LogError(
                    "Please do not send a float with any statType other than 'damageToTruck' as all others increment by 1.");
                break;
        }
    }

    public void ReceiveStat(StatType type)
    {
        switch (type)
        {
            case StatType.kill:
                _kills++;
                model.kills = _kills;
                break;
            case StatType.damage:
                Debug.LogError(
                    "Please send a float with statType 'damageToTruck' so that I know how much damage you did to the truck!");
                break;
            case StatType.powerup:
                _powerUp++;
                model.powerUp = _powerUp;
                break;
            case StatType.loot:
                _loot++;
                model.loot = _loot;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}