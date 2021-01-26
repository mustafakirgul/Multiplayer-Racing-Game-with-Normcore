using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RollPoolAndPlayerItemSave : ScriptableObject
{
    [SerializeField]
    List<ItemBase> RollPool = new List<ItemBase>();
    public List<ItemBase> m_RollPool => RollPool;

    public List<ItemBase> PlayerLoot = new List<ItemBase>();

    public List<ItemBase> PlayerLootToAdd = new List<ItemBase>();
}
