using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CarPhysicsParamsTemplate : ScriptableObject
{
    [SerializeField] [Range(0, 100)] private float m_meleePower;
    public float f_meleePower => m_meleePower;

    [SerializeField] [Range(0, 2)] private float m_topSpd;
    public float f_topSpd => m_topSpd;
    //Change this value when changing range as well
    public float maxPlayerTopSpd = 2;

    [SerializeField] [Range(0, 2)] private float m_acceleration;
    public float f_acceleration => m_acceleration;
    //Change this value when changing range as well
    public float maxPlayerAcceleration = 2;

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
    //Change this value when changing range as well
    public float maxPlayerHealthRef = 100;

    [SerializeField] [Range(0, 500)] private float m_rbWeight;
    public float f_rbWeight => m_rbWeight;
    //Change this value when changing range as well
    public float maxPlayerWeightRef = 500;

    [SerializeField] [Range(0, 20)] private float m_boostTimer;
    public float f_boostTimer => m_boostTimer;

    [SerializeField] [Range(0, 300)] private float m_boostForce;
    public float f_boostForce => m_boostForce;

    [SerializeField] [Range(0, 1)] private float m_defenseForce;
    public float f_defenseForce => m_defenseForce;
    //Change this value when changing range as well
    public float maxPlayerDefenseForce = 1;

    [SerializeField] [Range(1, 10)] private float m_ammoEfficiency;
    public float f_ammoEfficiency => m_ammoEfficiency;
}
