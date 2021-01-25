using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDataContainer : MonoBehaviour
{
    public int _buttonItemID;
    //Button 
    public void InjectButtonBuildDataToBuild()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        LootManager lootManager = LootManager.instance;

        switch (lootManager.playerObtainedLoot[_buttonItemID]._ItemType)
        {
            case ItemType.Weapon:

                lootManager.selected_buildLoadOutToView.Weapon
                = lootManager.playerObtainedLoot[_buttonItemID];
                //UI Image visualizaions
                uiManager.SelectedWeapon =
                lootManager.selected_buildLoadOutToView.Weapon.m_image;
                //UI Text for Item
                uiManager.ItemDescription.text =
                    lootManager.selected_buildLoadOutToView.Weapon.m_text;
                break;
            case ItemType.Armour:
                lootManager.selected_buildLoadOutToView.Armour
                = lootManager.playerObtainedLoot[_buttonItemID];
                //UI Image visualizaions
                uiManager.SelectedArmour =
                lootManager.selected_buildLoadOutToView.Armour.m_image;
                //UI Text for Item
                uiManager.ItemDescription.text =
                    lootManager.selected_buildLoadOutToView.Armour.m_text;
                break;
            case ItemType.Engine:
                lootManager.selected_buildLoadOutToView.Engine
                = lootManager.playerObtainedLoot[_buttonItemID];
                //UI Image visualizaions
                uiManager.SelectedEngine =
                lootManager.selected_buildLoadOutToView.Engine.m_image;
                //UI Text for Item
                uiManager.ItemDescription.text =
                    lootManager.selected_buildLoadOutToView.Engine.m_text;
                break;
            default:
                Debug.Log("Not a valid equipment type!");
                break;
        }
    }
}
