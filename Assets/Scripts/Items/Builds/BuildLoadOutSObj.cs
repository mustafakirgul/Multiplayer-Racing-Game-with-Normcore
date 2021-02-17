using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class BuildLoadOutSObj : ScriptableObject
{
    [Tooltip("Use this only to save loadouts for builds")]
    public BuildType buildType;

    public ItemBase Weapon;
    public ItemBase Armour;

    public ItemBase Engine;
    //Other data that needs to be added to the build once things start
}