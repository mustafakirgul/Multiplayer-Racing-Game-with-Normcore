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

        switch (lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID]._ItemType)
        {
            case ItemType.Weapon:

                lootManager.selected_buildLoadOutToView.Weapon
                = lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID];
                //UI Image visualizaions
                uiManager.SelectedWeapon.texture =
                lootManager.selected_buildLoadOutToView.Weapon.m_image;
                //UI Text for Item
                uiManager.ItemDescription.text =
                    lootManager.selected_buildLoadOutToView.Weapon.m_text;
                break;
            case ItemType.Armour:
                lootManager.selected_buildLoadOutToView.Armour
                = lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID];
                //UI Image visualizaions
                uiManager.SelectedArmour.texture =
                lootManager.selected_buildLoadOutToView.Armour.m_image;
                //UI Text for Item
                uiManager.ItemDescription.text =
                    lootManager.selected_buildLoadOutToView.Armour.m_text;
                break;
            case ItemType.Engine:
                lootManager.selected_buildLoadOutToView.Engine
                = lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID];
                //UI Image visualizaions
                uiManager.SelectedEngine.texture =
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
