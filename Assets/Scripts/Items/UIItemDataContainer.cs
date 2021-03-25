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
        BuildScrollSelector buildScrollSelector = FindObjectOfType<BuildScrollSelector>();
        LootManager lootManager = FindObjectOfType<LootManager>();

        switch (lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID]._ItemType)
        {
            case ItemType.Weapon:

                lootManager.selected_buildLoadOutToView.Weapon
                    = lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID];

                //Need to update the current ParamsObj with the data from the loot piece to have it reflected in the menu stats bar
                //when switching items need to reset back the data that was touched
                //Visualization for stats bar and ParamsSObj
                buildScrollSelector.currentSelectBuild.UpdateParamsFromUI(lootManager.selected_buildLoadOutToView.Weapon);
                //UI Image visualizaions
                uiManager.SelectedWeapon.texture =
                    lootManager.selected_buildLoadOutToView.Weapon.m_image;
                //UI Text for Item
                uiManager.ItemDescription.text =
                    lootManager.selected_buildLoadOutToView.Weapon.m_text;

                int WeaponIndex = (int)lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID].m_itemVisualIndex;

                uiManager.UpdateCarVisualModelsWeapons(WeaponIndex);


                //Set new flag to false
                lootManager.playerLootPoolSave.PlayerNewLabelLootFlags[_buttonItemID] = false;
                break;
            case ItemType.Armour:
                lootManager.selected_buildLoadOutToView.Armour
                    = lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID];

                //Visualization for stats bar and ParamsSObj
                buildScrollSelector.currentSelectBuild.UpdateParamsFromUI(lootManager.selected_buildLoadOutToView.Armour);
                //UI Image visualizaions
                uiManager.SelectedArmour.texture =
                    lootManager.selected_buildLoadOutToView.Armour.m_image;
                //UI Text for Item
                uiManager.ItemDescription.text =
                    lootManager.selected_buildLoadOutToView.Armour.m_text;

                int ArmourIndex = (int)lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID].m_itemVisualIndex;

                uiManager.UpdateCarVisualModelsArmour(ArmourIndex);

                //Set new flag to false
                lootManager.playerLootPoolSave.PlayerNewLabelLootFlags[_buttonItemID] = false;
                break;
            case ItemType.Engine:
                lootManager.selected_buildLoadOutToView.Engine
                    = lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID];

                //Visualization for stats bar and ParamsSObj
                buildScrollSelector.currentSelectBuild.UpdateParamsFromUI(lootManager.selected_buildLoadOutToView.Engine);
                //UI Image visualizaions
                uiManager.SelectedEngine.texture =
                    lootManager.selected_buildLoadOutToView.Engine.m_image;
                //UI Text for Item
                uiManager.ItemDescription.text =
                    lootManager.selected_buildLoadOutToView.Engine.m_text;

                //Model change
                int EngineIndex = (int)lootManager.playerLootPoolSave.PlayerLoot[_buttonItemID].m_itemVisualIndex;

                uiManager.UpdateCarVisualModelsEngines(EngineIndex);

                //Set new flag to false
                lootManager.playerLootPoolSave.PlayerNewLabelLootFlags[_buttonItemID] = false;
                break;
            default:
                Debug.Log("Not a valid equipment type!");
                break;
        }
    }
}