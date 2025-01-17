﻿using System.Collections.Generic;
using Items.TempMods;
using UnityEngine;

public enum BuildType
{
    Balanced,
    Speedy,
    Tank
}

public enum LootType
{
    Armour_One,
    Engine_One,
    Boost_One,
    Mine_Weapons,
    Homing_Missiles,
    Machine_Gun,
}

public enum PowerUpType
{
    Ammo,
    Boost,
    Defense,
    Health,
    Speed,
    SuperGun,
    TruckAttack,
}

public class LootManager : MonoBehaviour
{
    #region Singleton Logic

    public static LootManager instance = null;

    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    #endregion

    //Instead of a list of Itembase
    //Since only one item can be equipped at a time
    //Make this simple enough so it only accepts one at a time
    //Even for specific builds where 2 of a kind of item can be equipped
    //Still only use 1 extra type

    [Space] [Space] [Header("In-Game Applied LoadOut")]
    public BuildType current_buildType;

    [SerializeField] private BuildLoadOutSObj current_buildLoadOut;

    [Space] [Space] [Header("Garage Settings")] [SerializeField]
    public RollPoolAndPlayerItemSave playerLootPoolSave;

    public BuildType selected_buildType;
    public BuildLoadOutSObj selected_buildLoadOutToView;

    //Saved loadouts
    public BuildLoadOutSObj Balanced_buildLoadOut;
    public BuildLoadOutSObj Speedy_buildLoadOut;
    public BuildLoadOutSObj Tank_buildLoadOut;

    public List<CarPhysicsParamsSObj> CarPhysicsParams = new List<CarPhysicsParamsSObj>();

    public List<CarPhysicsParamsTemplate> CarPhysicsTemplates = new List<CarPhysicsParamsTemplate>();

    [SerializeField]

    //[SerializeField]
    public int numberOfLootRolls;

    [SerializeField] private LootDecoder lootDecoder;

    public Vector3 VisualIndex = Vector3.zero;
    public AudioPlayer lootOpenSound;

    private void Awake()
    {
        SingletonCheck();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (lootOpenSound == null) lootOpenSound = GetComponent<AudioPlayer>();
        if (current_buildLoadOut == null)
        {
            current_buildLoadOut = Balanced_buildLoadOut;
            SelectBuildToDisplay(Balanced_buildLoadOut);
        }
        else
        {
            SelectBuildToDisplay(current_buildLoadOut);
        }

        //numberOfLootRolls = 0;
    }

    public TempItemSObj DecodePowerUp(int ItemID)
    {
        if (ItemID == 0)
        {
            //Creates a powerup with no effect
            TempItemSObj DummyPU = playerLootPoolSave.PlayerPowerUps[playerLootPoolSave.PlayerPowerUps.Count - 1];
            Debug.LogWarning(
                "Item is already picked up! If this is the prespawner, make sure it is disabled after spawning items");
            return DummyPU;
        }

        int Index = Mathf.Abs(ItemID);
        //Note index for List starts still at 0 so minus 1 to start from the 1st element in the PU list
        TempItemSObj DecodedPU = playerLootPoolSave.PlayerPowerUps[Index - 1];
        return DecodedPU;
    }

    public Vector3 VisualModelIndex()
    {
        {
            VisualIndex =
                new Vector3(current_buildLoadOut.Weapon.m_itemVisualIndex,
                    current_buildLoadOut.Armour.m_itemVisualIndex,
                    current_buildLoadOut.Engine.m_itemVisualIndex);

            return VisualIndex;
        }
    }

    public void ActivateVisualIndex()
    {
        VisualModelIndex();
    }

    public void RollForLoot()
    {
        if (numberOfLootRolls != 0)
        {
            for (int i = 0; i < numberOfLootRolls; i++)
            {
                //To do add more sophisticated loot drop system
                //ItemBase itemToAdd = 
                //    ReferenceLootPool[Random.Range(0, ReferenceLootPool.Count)];
                //playerObtainedLoot.Add(itemToAdd);
                //Notification for player who have rolled for loot at the end of the round

                ItemBase itemToAdd =
                    playerLootPoolSave.m_RollPool[Random.Range(0, playerLootPoolSave.m_RollPool.Count)];
                playerLootPoolSave.PlayerLootToAdd.Add(itemToAdd);

                ConfigureDecoder(itemToAdd);
            }

            lootDecoder.StartSequence();
            GameSceneManager.instance.DisableEndSplashes();
            lootDecoder.canCheck = true;
        }

        //Reset once roll is complete
        numberOfLootRolls = 0;
    }

    private void ConfigureDecoder(ItemBase lootReference)
    {
        //Reference an empty decoder slot to decode
        GameObject LootDisplay = Instantiate(lootDecoder.LootDecoderUnitToSpawn);

        //Configure slot to decode with information of loot
        DecoderDataReference itemDataReference = LootDisplay.GetComponentInChildren<DecoderDataReference>();

        itemDataReference.LootImage.texture = lootReference.m_image;
        itemDataReference.LootName.text = lootReference.name;
        itemDataReference.LootDescription.text = lootReference.m_text;

        //Add the configured UI display to the list under the decoder
        lootDecoder.LootDecoderUnits.Add(LootDisplay);
    }

    public void SelectBuildToDisplay(BuildLoadOutSObj buildToDisplay)
    {
        selected_buildLoadOutToView = buildToDisplay;
    }

    public BuildLoadOutSObj ObtainCurrentBuild()
    {
        return current_buildLoadOut;
    }

    //For UI build buttons to use
    public void DeploySelectedBuild()
    {
        current_buildLoadOut = selected_buildLoadOutToView;
        current_buildType = selected_buildLoadOutToView.buildType;
    }

    public void RetreiveBuild(int buildIndex)
    {
        switch (buildIndex)
        {
            case 0:
                SelectBuildToDisplay(Balanced_buildLoadOut);
                break;
            case 1:
                SelectBuildToDisplay(Speedy_buildLoadOut);
                break;
            case 2:
                SelectBuildToDisplay(Tank_buildLoadOut);
                break;
            default:
                Debug.Log("No build selected!");
                break;
        }
    }

    //For UI Use
    public BuildLoadOutSObj ObtainLastLoadedBuild()
    {
        return current_buildLoadOut;
    }
}