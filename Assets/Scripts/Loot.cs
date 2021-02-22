using System;
using Normal.Realtime;
using UnityEngine;

public class Loot : RealtimeComponent<LootModel>
{
    [Space(10)] public int id;
    public int collectedBy;
    private LootContainer lootContainer;

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
            }

            currentModel.idDidChange += IdDidChange;
            currentModel.collectedByDidChange += CollectedByDidChange;
        }
    }

    private void Awake()
    {
        lootContainer = GetComponent<LootContainer>();
    }

    public void Update()
    {
        //Local Instance of collection not updating collection ID correctly for some reason
        //Only happening to host not connected players
        if (model == null || lootContainer == null) return;
        IDChanged();
    }

    public void SetID(int _id)
    {
        model.id = _id;
    }

    public void SetCollectedBy(int _collectedBy)
    {
        model.collectedBy = _collectedBy;
        lootContainer.DisplayCollectionMessage();
    }

    private void CollectedByDidChange(LootModel lootModel, int value)
    {
        CollectedByChanged();
    }

    private void CollectedByChanged()
    {
        collectedBy = model.collectedBy;
        lootContainer.DisplayCollectionMessage();
    }

    private void IdDidChange(LootModel lootModel, int value)
    {
        IDChanged();
    }

    private void IDChanged()
    {
        id = model.id;
        lootContainer.id = id;
    }
}