using System;
using Normal.Realtime;
using UnityEngine;

public class Loot : RealtimeComponent<LootModel>
{
    [Space(10)] public int id;
    public int collectedBy;
    private LootContainer _container => transform.GetComponent<LootContainer>();

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
            
            _container.SetID(currentModel.id);
            Debug.LogWarning("ID set in replace event: " + currentModel.id);
            currentModel.idDidChange += IDChanged;
            currentModel.collectedByDidChange += CollectedByChanged;
        }
    }

    public void SetID(int _id)
    {
        if (model != null)
            model.id = _id;
        Debug.LogWarning("ID set in loot_setid: " + _id);
    }

    public int SetCollectedBy(int _collectedBy)
    {
        if (_collectedBy > 0 && model.collectedBy == 0)
        {
            model.collectedBy = _collectedBy;
        }

        return model.collectedBy;
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