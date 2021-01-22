using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class ItemBase : ScriptableObject
{
    public ItemType _ItemType;

    //Read only data
    [SerializeField] private float f_attack = 0f, f_defense = 0f, f_speed = 0f, f_amount = 0f;
    private int i_subType = 0;

    [SerializeField] private string itemText = "";

    [SerializeField] private Texture2D itemImage = null;

    public float m_Attack => f_attack;
    public float m_Defense => f_defense;
    public float m_Speed => f_speed;
    public float m_amount => f_amount;
    public int m_subType => i_subType;

    public string m_text => itemText;

    public Texture2D m_image => itemImage;

    [SerializeField] private GameObject m_CosmeticModelToBeApplied;
}