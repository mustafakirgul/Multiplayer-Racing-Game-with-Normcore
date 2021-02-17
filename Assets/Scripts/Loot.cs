﻿using Normal.Realtime;
using UnityEngine;

public class Loot : RealtimeComponent<LootModel>
{
    [Space(10)] public int id;
    public int collectedBy;

    protected override void OnRealtimeModelReplaced(LootModel previousModel, LootModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);

        if (previousModel != null)
        {
            previousModel.idDidChange -= IdDidChange;
            previousModel.collectedByDidChange -= CollectedByDidChange;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                SetID(id);
                currentModel.collectedBy = -1;
            }

            UpdateLoot();
            currentModel.idDidChange += IdDidChange;
            currentModel.collectedByDidChange += CollectedByDidChange;
        }
    }

    private void UpdateLoot()
    {
        IDChanged();
        CollectedByChanged();
    }

    public void SetID(int _id)
    {
        model.id = _id;
    }

    public void SetCollectedBy(int _collectedBy)
    {
        model.collectedBy = _collectedBy;
        GetComponent<LootContainer>().DisplayCollectionMessage();
    }

    private void CollectedByDidChange(LootModel lootModel, int value)
    {
        CollectedByChanged();
    }

    private void CollectedByChanged()
    {
        collectedBy = model.collectedBy;
    }

    private void IdDidChange(LootModel lootModel, int value)
    {
        IDChanged();
    }

    private void IDChanged()
    {
        id = model.id;
    }
}