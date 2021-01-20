using UnityEngine;
[CreateAssetMenu]
public class ItemBase : ScriptableObject
{

    public ItemType _ItemType;

    //Read only data
    [SerializeField]
    private float f_attack, f_health, f_speed, f_amount;
    private int i_subType;

    public float m_Attack => f_attack;
    public float m_Health => f_health;
    public float m_Speed => f_speed;
    public float m_amount => f_amount;
    public int m_subType => i_subType;

    [SerializeField]
    private GameObject m_CosmeticModelToBeApplied;
}
