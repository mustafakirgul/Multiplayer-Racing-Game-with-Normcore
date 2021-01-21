using System;
using Normal.Realtime;
using UnityEngine;

public class Loot : RealtimeComponent<LootModel>
{
    [Space(10)] public int id;
    public int collectedBy;
    private LootModel _model;

    protected override void OnRealtimeModelReplaced(LootModel previousModel, LootModel currentModel)
    {
        //base.OnRealtimeModelReplaced(previousModel, currentModel); // do we need this?

        if (previousModel != null)
        {
            previousModel.idDidChange -= IDChanged;
            previousModel.collectedByDidChange -= CollectedByChanged;
        }

        if (currentModel != null)
        {
            currentModel.idDidChange += IDChanged;
            currentModel.collectedByDidChange += CollectedByChanged;
            _model = currentModel;
            if (currentModel.isFreshModel)
            {
                id = _model.id;
            }
        }
    }

    public int SetID(int _id)
    {
        if (_model != null && _model.id == 0)
        {
            _model.id = _id;
            return _model.id;
        }
        return 0;
    }

    public int SetCollectedBy(int _collectedBy)
    {
        if (_collectedBy > 0 && _model.collectedBy == 0)
        {
            _model.collectedBy = _collectedBy;
        }

        return _model.collectedBy;
    }

    private void CollectedByChanged(LootModel lootModel, int value)
    {
        collectedBy = value;
    }

    private void IDChanged(LootModel lootModel, int value)
    {
        id = value;
    }
}