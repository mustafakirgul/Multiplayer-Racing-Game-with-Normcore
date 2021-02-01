using System.Collections.Generic;
using UnityEngine;

public enum ItemType { None, Weapon, Armour, Engine, Other }
public class ItemDataProcessor : MonoBehaviour
{
    [SerializeField]
    private Player m_playerHealth;
    [SerializeField]
    private NewCarController m_playerCarController;

    [SerializeField]
    private float mf_ItemMeleeAttack;
    [SerializeField]
    private float mf_ItemArmour;
    [SerializeField]
    private float mf_ItemSpeed;
    [SerializeField]
    private float mf_ItemOther;
    [SerializeField]
    private float mf_maxItemCount;

    [SerializeField]
    public BuildLoadOutSObj current_buildLoadOut;

    private void Awake()
    {
        //Modifications for player health/defense/armour
        m_playerHealth = GetComponent<Player>();
        //Modifications for player speed/weapons
        m_playerCarController = GetComponent<NewCarController>();
    }

    //Use this for temporary items that will boost the attack or defense
    //of builds in game
    public void ProcessInGameItemData(ItemBase ItemPickUp)
    {
        switch (ItemPickUp._ItemType)
        {
            case ItemType.None:
                break;
            case ItemType.Weapon:
                mf_ItemMeleeAttack += ItemPickUp.m_Attack;

                //Additional Weapon logic here
                break;
            case ItemType.Armour:
                mf_ItemArmour += ItemPickUp.m_Defense;
                break;
            case ItemType.Engine:
                mf_ItemSpeed += ItemPickUp.m_Speed;
                break;
            case ItemType.Other:
                //mf_ItemOther += ItemPickUp.m_Speed;
                break;
            default:
                break;
        }
    }
    public void ProcessLoadOutData(ItemBase ItemPickUp)
    {
        switch (ItemPickUp._ItemType)
        {
            case ItemType.None:
                break;
            case ItemType.Weapon:
                mf_ItemMeleeAttack = ItemPickUp.m_Attack;
                m_playerCarController.meleeDamageModifier = mf_ItemMeleeAttack;
                //TO DO need to add weapon projectile for ranged weapons etc.
                m_playerCarController.SetCurrentWeapon(
                    ItemPickUp.GetProjectileForWeapon, 
                    ItemPickUp.m_fireRate,
                    //Temp Truck Damage Modifier
                    ItemPickUp.m_Attack
                    );
                break;
            case ItemType.Armour:
                mf_ItemArmour = ItemPickUp.m_Defense;
                m_playerHealth.armourDefenseModifier = mf_ItemArmour;
                break;
            case ItemType.Engine:
                mf_ItemSpeed = ItemPickUp.m_Speed;
                m_playerCarController.HandlingModifier = mf_ItemSpeed;
                //TODO: accleration and top speed
                //m_playerCarController.accelerationModifier
                //m_playerCarController.MaxSpeedModifier;
                break;
            case ItemType.Other:
                Debug.Log("Nothing to Update");
                break;
            default:
                break;
        }
    }

    //This is for permanent loot loadouts
    public void ObtainLoadOutData(BuildLoadOutSObj CurrentLoadOut)
    {
        //TO DO: Update data to Car Controller and other areas where pretinent
        if (CurrentLoadOut != null)
        {
            //Use it to Collect the DataNeeded to Update the CarController
            //Update the car controller
            //Note LoadOut Info does not need to be saved here
            //Only crunch the numbers
            ProcessLoadOutData(CurrentLoadOut.Armour);
            ProcessLoadOutData(CurrentLoadOut.Engine);
            ProcessLoadOutData(CurrentLoadOut.Weapon);
            UpdateExtraData(CurrentLoadOut);
        }
        else
        {
            Debug.LogError("No Build LoadOut Sobj detected!");
        }
    }

    public void UpdateExtraData(BuildLoadOutSObj CurrentLoadOut)
    {
        switch (CurrentLoadOut.buildType)
        {
            case BuildType.Balanced:
                //Add extra balance build stats if any
                break;
            case BuildType.Speedy:
                //Add extra speedy build stats if any
                break;
            case BuildType.Tank:
                //Add extra tank build stats if any
                break;
            default:
                break;
        }
    }
}
