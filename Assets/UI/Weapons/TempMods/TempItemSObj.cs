using UnityEngine;

namespace Items.TempMods
{
    [CreateAssetMenu]
    public class TempItemSObj : ScriptableObject
    {
        public PowerUpType powerUpType;

        public float PrimaryModifierValue;

        public float ExtraWeaponModifierValue;

        public GameObject projectileType;

        public GameObject MeshAppearance;
    }
}