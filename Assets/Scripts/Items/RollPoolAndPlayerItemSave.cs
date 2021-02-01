using System.Collections;
using System.Collections.Generic;
using Items.TempMods;
using UnityEngine;

[CreateAssetMenu]
public class RollPoolAndPlayerItemSave : ScriptableObject
{
    [SerializeField] List<ItemBase> RollPool = new List<ItemBase>();
    public List<ItemBase> m_RollPool => RollPool;

    public List<ItemBase> PlayerLoot = new List<ItemBase>();

    public List<bool> PlayerNewLabelLootFlags = new List<bool>();

    public List<ItemBase> PlayerLootToAdd = new List<ItemBase>();

    public List<TempItemSObj> PlayerPowerUps = new List<TempItemSObj>();
}