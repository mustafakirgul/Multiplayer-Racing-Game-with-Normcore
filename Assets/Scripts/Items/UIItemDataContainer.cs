using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDataContainer : MonoBehaviour
{
    public int _buttonItemID;
    //Button 
    public void InjectButtonBuildDataToBuild()
    {
        switch (LootManager.instance.
            playerObtainedLoot[_buttonItemID]._ItemType)
        {
            case ItemType.Weapon:

                LootManager.instance.selected_buildLoadOutToView.Weapon
                = LootManager.instance.playerObtainedLoot[_buttonItemID];
                break;
            case ItemType.Armour:
                LootManager.instance.selected_buildLoadOutToView.Armour
                = LootManager.instance.playerObtainedLoot[_buttonItemID];
                break;
            case ItemType.Engine:
                LootManager.instance.selected_buildLoadOutToView.Engine
                = LootManager.instance.playerObtainedLoot[_buttonItemID];
                break;
            default:
                Debug.Log("Not a valid equipment type!");
                break;
        }
    }
}
