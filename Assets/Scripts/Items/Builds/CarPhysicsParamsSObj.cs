using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CarPhysicsParamsSObj : ScriptableObject
{
    [SerializeField] [Range(0, 500)] private float m_FowardSpd;
    public float f_FowardSpd => m_FowardSpd;

    [SerializeField] [Range(0, 200)] private float m_ReverseSpd;
    public float f_ReverseSpd => m_ReverseSpd;

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

    [SerializeField] [Range(0, 300)] private float m_rbWeight;
    public float f_rbWeight => m_rbWeight;

    [SerializeField] [Range(0, 20)] private float m_boostTimer;
    public float f_boostTimer => m_boostTimer;

    [SerializeField] [Range(0, 300)] private float m_boostForce;
    public float f_boostForce => m_boostForce;

    [SerializeField] [Range(0, 10)] private float m_defenseForce;
    public float f_defenseForce => m_defenseForce;
}