using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TempItemSObj : ScriptableObject
{
    public PowerUpType powerUpType;

    public float PrimaryModifierValue;

    public float ExtraWeaponModifierValue;

    public GameObject projectileType;

    public GameObject MeshAppearance;
}
