using UnityEngine;

[CreateAssetMenu]
public class ItemBase : ScriptableObject
{
    public ItemType _ItemType;

    //Read only data
    [SerializeField] private float f_attack = 0f, f_defense = 0f, f_speed = 0f, f_amount = 0f, f_fireRate = 0f, f_itemVisualIndex;
    private int i_subType = 0;

    [SerializeField] private string itemText = "";

    public Texture2D itemImage;

    public float m_Attack => f_attack;
    public float m_Defense => f_defense;
    public float m_Speed => f_speed;
    public float m_amount => f_amount;
    public int m_subType => i_subType;
    public float m_fireRate => f_fireRate;
    public string m_text => itemText;
    public float m_itemVisualIndex => f_itemVisualIndex;

    public Texture2D m_image => itemImage;

    public GameObject WeaponProjectile;
    public GameObject GetProjectileForWeapon => WeaponProjectile;

    [SerializeField] private GameObject m_CosmeticModelToBeApplied;
}