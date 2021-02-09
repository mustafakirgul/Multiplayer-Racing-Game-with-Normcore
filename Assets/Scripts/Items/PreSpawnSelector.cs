using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreSpawnSelector : MonoBehaviour
{
    public bool isPowerUp;

    public PowerUpType powerUpTypeSelected;

    public LootType lootTypeSelected;

    private Loot LootSelected;

    public Vector3 PUSpawnReferencePoint;

    private void OnValidate()
    {
        LootSelected = GetComponent<Loot>();
        AssignIDToSpawner();
        GetLocationDatainV3();
    }

    private void Start()
    {
        AssignIDToSpawner();
    }

    private void GetLocationDatainV3()
    {
        PUSpawnReferencePoint = new Vector3(this.transform.position.x,
                                              this.transform.position.y,
                                              this.transform.position.z);
    }

    private void AssignIDToSpawner()
    {
        if (isPowerUp)
        {
            switch (powerUpTypeSelected)
            {
                case PowerUpType.Ammo:
                    LootSelected.id = -1;
                    break;
                case PowerUpType.Boost:
                    LootSelected.id = -4;
                    break;
                case PowerUpType.Defense:
                    LootSelected.id = -5;
                    break;
                case PowerUpType.Health:
                    LootSelected.id = -2;
                    break;
                case PowerUpType.Speed:
                    LootSelected.id = -6;
                    break;
                case PowerUpType.SuperGun:
                    LootSelected.id = -3;
                    break;
                case PowerUpType.TruckAttack:
                    LootSelected.id = -7;
                    break;
                default:
                    LootSelected.id = -8;
                    break;
            }
        }
        else
        {
            switch (lootTypeSelected)
            {
                case LootType.Armour_One:
                    //Do nothing for now, does not need to spawn specific loot right now
                    break;
                default:
                    Debug.Log("No pre determined loot to spawn");
                    break;
            }
        }
    }
}