﻿using System;
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
                currentModel.killer = -1;
            }
        }
    }

    private void Start()
    {
        foreach (var p in FindObjectsOfType<Player>())
        {
            if (p.statsEntity == null)
            {
                if (p.realtimeView.ownerIDInHierarchy == realtimeView.ownerIDInHierarchy)
                {
                    p.statsEntity = this;
                }
            }
        }
    }

    private void Update()
    {
        _kills = model.kills;
        _damageToTruck = model.damageToTruck;
        _powerUp = model.powerUp;
        _loot = model.loot;

        /*//To be removed in final build
        if (Input.GetKeyDown(KeyCode.N))
        {
            model.loot++;
            LootManager.instance.numberOfLootRolls++;
        }*/
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

    public void LoseLoot()
    {
        if (realtimeView.isOwnedLocallyInHierarchy)
        {
            int temp = Mathf.Clamp(model.loot - 1, 0, 999999);
            model.loot = temp;
        }
    }

    public void ResetStats()
    {
        model.loot = 0;
        model.kills = 0;
        model.killer = -1;
        model.powerUp = 0;
        model.damageToTruck = 0;
    }

    public void ReceiveKiller(int killer)
    {
        if (model.isOwnedLocallyInHierarchy)
        {
            model.killer = killer;
        }
    }

    public ComparisonTableColumn ReturnStats()
    {
        return new ComparisonTableColumn(model.kills, Convert.ToInt32(model.damageToTruck), model.powerUp, model.loot);
    }

    public void ReceiveStat(StatType type)
    {
        switch (type)
        {
            case StatType.kill:
                model.kills++;
                model.killer = -1;
                break;
            case StatType.damage:
                Debug.LogError(
                    "Please send a float with statType 'damageToTruck' so that I know how much damage you did to the truck!");
                break;
            case StatType.powerup:
                model.powerUp++;
                break;
            case StatType.loot:
                model.loot++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}

public struct ComparisonTableColumn
{
    public ComparisonTableColumn(int kills, int damage, int powerUp, int loot)
    {
        this.kills = kills;
        this.damage = damage;
        this.powerUp = powerUp;
        this.loot = loot;
    }

    public int kills;
    public int damage;
    public int powerUp;
    public int loot;
}