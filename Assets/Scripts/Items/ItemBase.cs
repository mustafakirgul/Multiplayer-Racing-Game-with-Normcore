using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu]
public class ItemBase : ScriptableObject
{
    public ItemType _ItemType;
    //Read only data
    [SerializeField]
    private float f_attack, f_defense, f_speed, f_amount;
    private int i_subType;

    [SerializeField]
    private string itemText;

    [SerializeField]
    private Image itemImage;

    public float m_Attack => f_attack;
    public float m_Defense => f_defense;
    public float m_Speed => f_speed;
    public float m_amount => f_amount;
    public int m_subType => i_subType;

    public string m_text => itemText;

    public Image m_image => itemImage;

    [SerializeField]
    private GameObject m_CosmeticModelToBeApplied;
}
