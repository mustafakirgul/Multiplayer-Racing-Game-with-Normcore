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
            if (currentModel.isFreshModel)
            {
                id = currentModel.id;
                collectedBy = currentModel.collectedBy;
            }

            currentModel.idDidChange += IDChanged;
            currentModel.collectedByDidChange += CollectedByChanged;
            _model = currentModel;
        }
    }

    public int SetID(int _id)
    {
        if (_model.id == 0)
        {
            _model.id = _id;
        }

        return _model.id;
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