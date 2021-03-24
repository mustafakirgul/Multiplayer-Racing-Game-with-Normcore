using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CarPhysicsParamsSObj : ScriptableObject
{
    [SerializeField] [Range(0, 100)] private float m_meleePower;
    public float f_meleePower => m_meleePower;

    [SerializeField] [Range(0, 2)] private float m_topSpd;
    public float f_topSpd => m_topSpd;

    [SerializeField] [Range(0, 2)] private float m_acceleration;
    public float f_acceleration => m_acceleration;

    [SerializeField] [Range(0, 2)] private float m_ReverseAccel;
    public float f_ReverseAccel => m_ReverseAccel;

    [SerializeField] [Range(0, 200)] private float m_TurnSpd;
    public float f_TurnSpd => m_TurnSpd;

    [SerializeField] [Range(0, 100)] private float m_TurnFwdSpd;
    public float f_TurnFwdSpd => m_TurnFwdSpd;

    [SerializeField] [Range(0, 1)] private float m_BrakeForce;
    public float f_BrakeForce => m_BrakeForce;

    [SerializeField] [Range(0, 100)] private float m_Gravity;
    public float f_Gravity => m_Gravity;

    [SerializeField] [Range(0, 100)] private float m_maxPlayerHealth;
    public float f_maxPlayerHealth => m_maxPlayerHealth;

    [SerializeField] [Range(0, 100)] private float m_rbWeight;
    public float f_rbWeight => m_rbWeight;

    [SerializeField] [Range(0, 20)] private float m_boostTimer;
    public float f_boostTimer => m_boostTimer;

    [SerializeField] [Range(0, 300)] private float m_boostForce;
    public float f_boostForce => m_boostForce;

    [SerializeField] [Range(0, 1)] private float m_defenseForce;
    public float f_defenseForce => m_defenseForce;

    [SerializeField] [Range(1, 10)] private float m_ammoEfficiency;
    public float f_ammoEfficiency => m_ammoEfficiency;

    [SerializeField]
    private float tempMelee, tempTopSpd, tempAccel, tempRAccel, tempTurnSpd, tempTurnFwdSpd, tempBrakeForce,
        tempGravity, tempMaxPlayerHP, tempWeight, tempBoostTimer, tempBoostForce,
        tempDefenseForce, tempAmmoEfficiency;

    public void ResetData(CarPhysicsParamsTemplate templateData)
    {
        if (templateData != null)
        {
            Debug.Log("Running Sobj Logic");
            tempMelee = templateData.f_meleePower;
            tempTopSpd = templateData.f_topSpd;
            tempAccel = templateData.f_acceleration;
            tempRAccel = templateData.f_ReverseAccel;
            tempTurnSpd = templateData.f_TurnSpd;
            tempTurnFwdSpd = templateData.f_TurnFwdSpd;
            tempBrakeForce = templateData.f_BrakeForce;
            tempGravity = templateData.f_Gravity;
            tempMaxPlayerHP = templateData.f_maxPlayerHealth;
            tempWeight = templateData.f_rbWeight;
            tempBoostTimer = templateData.f_boostTimer;
            tempBoostForce = templateData.f_boostForce;
            tempDefenseForce = templateData.f_defenseForce;
            tempAmmoEfficiency = templateData.f_ammoEfficiency;

            m_meleePower = tempMelee;
            m_topSpd = tempTopSpd;
            m_acceleration = tempAccel;
            m_ReverseAccel = tempRAccel;
            m_TurnSpd = tempTurnSpd;
            m_TurnFwdSpd = tempTurnFwdSpd;
            m_BrakeForce = tempBrakeForce;
            m_Gravity = tempGravity;
            m_maxPlayerHealth = tempMaxPlayerHP;
            m_rbWeight = tempWeight;
            m_boostTimer = tempBoostTimer;
            m_boostForce = tempBoostForce;
            m_defenseForce = tempDefenseForce;
            m_ammoEfficiency = tempAmmoEfficiency;
        }
    }

    public void UpdateParamsFromUI(ItemBase item)
    {
        //Important: each itemType is only set up to modify specific stats of things
        //max of 5 to 6 stats each only
        //Best to refer to ItemDataProcessor for reference
        //This is very tedious but creates alot of variations in weapons

        switch (item._ItemType)
        {
            case ItemType.Weapon:
                //Weapon affects: Projectiles, meleeattack, acceleration, topspeed, weight, turnspeed
                if (item.m_Attack != 0)
                    SetMeleePower(item.m_Attack);
                if (item.m_acceleration != 0)
                    SetAccleration(item.m_acceleration);
                if (item.m_TopSpeed != 0)
                    SetTopSpd(item.m_TopSpeed);
                if (item.m_weight != 0)
                    SetWeight(item.m_weight);
                if (item.m_turnSpd != 0)
                    SetTurnSpd(item.m_turnSpd);
                break;
            case ItemType.Armour:
                //Armour affects: MeleeAttack, Defense, Acceleration, TopSpeed, Weight, TurnSpeed
                if (item.m_Attack != 0)
                    SetMeleePower(item.m_Attack);
                if (item.m_Defense != 0)
                    SetDefenseForce(item.m_Defense);
                if (item.m_acceleration != 0)
                    SetAccleration(item.m_acceleration);
                if (item.m_TopSpeed != 0)
                    SetTopSpd(item.m_TopSpeed);
                if (item.m_weight != 0)
                    SetWeight(item.m_weight);
                if (item.m_health != 0)
                    SetMaxPlayerHealth(item.m_health);
                break;
            case ItemType.Engine:
                //Engine affects: MeleeAttack, Acceleration, TopSpeed, Weight, TurnSpeed
                if (item.m_Attack != 0)
                    SetMeleePower(item.m_Attack);
                if (item.m_Defense != 0)
                    SetDefenseForce(item.m_Defense);
                if (item.m_acceleration != 0)
                    SetAccleration(item.m_acceleration);
                if (item.m_TopSpeed != 0)
                    SetTopSpd(item.m_TopSpeed);
                if (item.m_weight != 0)
                    SetWeight(item.m_weight);
                if (item.m_turnSpd != 0)
                    SetTurnSpd(item.m_turnSpd);
                break;
        }
    }

    public void SetMeleePower(float value)
    {
        //if (tempMelee != 0)
        {
            m_meleePower = tempMelee;
        }
        m_meleePower += value;
    }
    public void SetTopSpd(float value)
    {
        //if (tempTopSpd != 0)
        {
            m_topSpd = tempTopSpd;
        }
        m_topSpd += value;
    }
    public void SetAccleration(float value)
    {
        //if (tempAccel != 0)
        {
            m_acceleration = tempAccel;
        }
        m_acceleration += value;
    }

    public void SetReverseAcceleration(float value)
    {
        //if (tempRAccel != 0)
        {
            m_ReverseAccel = tempRAccel;
        }
        m_ReverseAccel += value;
    }

    public void SetTurnSpd(float value)
    {
        //if (tempTurnSpd != 0)
        {
            m_TurnSpd = tempTurnSpd;
        }
        m_TurnSpd += value;
    }

    public void SetTurnFwdSpd(float value)
    {
        //if (tempTurnSpd != 0)
        {
            m_TurnFwdSpd = tempTurnFwdSpd;
        }
        m_TurnFwdSpd += value;
    }


    public void SetBrakeForce(float value)
    {
        //if (tempBrakeForce != 0)
        {
            m_BrakeForce = tempBrakeForce;
        }
        m_BrakeForce += value;
    }

    public void SetGravity(float value)
    {
        //if (tempGravity != 0)
        {
            m_Gravity = tempGravity;
        }
        m_Gravity += value;
    }

    public void SetMaxPlayerHealth(float value)
    {
        //if (tempMaxPlayerHP != 0)
        {
            m_maxPlayerHealth = tempMaxPlayerHP;
        }
        m_maxPlayerHealth += value;
    }

    public void SetWeight(float value)
    {
        //if (tempWeight != 0)
        {
            m_rbWeight = tempWeight;
        }
        m_rbWeight += value;
    }

    public void SetBoostTimer(float value)
    {
        //if (tempBoostTimer != 0)
        {
            m_boostTimer = tempBoostTimer;
        }
        m_boostTimer = value;
    }

    public void SetBoostForce(float value)
    {
        //if (tempBoostForce != 0)
        {
            m_boostForce = tempBoostForce;
        }
        m_boostForce += value;
    }
    public void SetDefenseForce(float value)
    {
        //if (tempDefenseForce != 0)
        {
            m_defenseForce = tempDefenseForce;
        }
        m_defenseForce += value;
    }
    public void SetAmmoEfficiency(float value)
    {
        //if (tempAmmoEfficiency != 0)
        {
            m_ammoEfficiency = tempAmmoEfficiency;
        }
        m_ammoEfficiency = value;
    }
}