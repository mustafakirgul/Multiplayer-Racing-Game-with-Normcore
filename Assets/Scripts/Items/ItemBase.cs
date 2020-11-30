﻿using UnityEngine;
[CreateAssetMenu]
public class ItemBase : ScriptableObject
{

    public ItemType _ItemType;

    //Read only data
    [SerializeField]
    private float f_attack, f_defense, f_health, f_speed;

    public float m_Attack => f_attack;
    public float m_Defense => f_defense;
    public float m_Health => f_health;
    public float m_Speed => f_speed;

    [SerializeField]
    private GameObject Model;
}
