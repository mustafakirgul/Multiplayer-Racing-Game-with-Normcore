﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildType
{
    Balanced,
    Speedy,
    Tank
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
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    //Instead of a list of Itembase
    //Since only one item can be equipped at a time
    //Make this simple enough so it only accepts one at a time
    //Even for specific builds where 2 of a kind of item can be equipped
    //Still only use 1 extra type

    [Space]
    [Space]
    [Header("In-Game Applied LoadOut")]
    public BuildType current_buildType;
    [SerializeField]
    private BuildLoadOutSObj current_buildLoadOut;

    [Space]
    [Space]
    [Header("Garage Settings")]
    [SerializeField]

    private List<ItemBase> ReferenceLootPool = new List<ItemBase>();
    public List<ItemBase> playerObtainedLoot = new List<ItemBase>();

    public BuildType selected_buildType;
    public BuildLoadOutSObj selected_buildLoadOutToView;

    //Saved loadouts
    public BuildLoadOutSObj Balanced_buildLoadOut;
    public BuildLoadOutSObj Speedy_buildLoadOut;
    public BuildLoadOutSObj Tank_buildLoadOut;

    [SerializeField]
    private int numberOfLootRolls;
    private void Awake()
    {
        SingletonCheck();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (current_buildLoadOut == null)
        {
            SelectBuildToDisplay(Balanced_buildLoadOut);
        }
        else
        {
            SelectBuildToDisplay(current_buildLoadOut);
        }

        numberOfLootRolls = 0;
    }

    public void DetermineTypeOfPickUP(int playerID, int pickupRoll)
    {
        if (pickupRoll > 0) 
        {
            numberOfLootRolls++;
        } 
        else
        {
            Debug.Log("player "+ playerID + " obtained a powerup! ");
        }
    }

    public void RollForLoot()
    {
        for (int i = 0; i < numberOfLootRolls; i++)
        {
            //To do add more sophisticated loot drop system
            ItemBase itemToAdd =
                ReferenceLootPool[Random.Range(0, ReferenceLootPool.Count)];
            playerObtainedLoot.Add(itemToAdd);
            //Notification for player who have rolled for loot at the end of the round
        }
        //Reset once roll is complete
        numberOfLootRolls = 0;
    }

    public void SelectBuildToDisplay(BuildLoadOutSObj buildToDisplay)
    {
        selected_buildLoadOutToView = buildToDisplay;
    }

    public BuildLoadOutSObj ObatinCurrentBuild()
    {
        if (current_buildLoadOut != null)
        {
            return current_buildLoadOut;
        }
        else
        {
            return null;
        }
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
