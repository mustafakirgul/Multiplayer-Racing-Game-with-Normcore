using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemBase : ScriptableObject
{
    public enum ItemType { None, Attack, Armour, Body, Speed }
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

public class ItemDataProcessor : MonoBehaviour
{
    [SerializeField]
    private Player m_player;
    [SerializeField]
    private List<ItemBase> m_listOfItems;
    [SerializeField]
    private float mf_ItemMeleeAttack;
    [SerializeField]
    private float mf_ItemDefense;
    [SerializeField]
    private float mf_ItemHealth;
    [SerializeField]
    private float mf_ItemSpeed;
    [SerializeField]
    private float mf_maxItemCount;

    private void Awake()
    {
        m_player = GetComponent<Player>();
        processAllItemDate();
    }

    void processAllItemDate()
    {
        for (int i = 0; i < m_listOfItems.Count; i++)
        {
            if (m_listOfItems[i]._ItemType == ItemBase.ItemType.Attack)
            {
                mf_ItemMeleeAttack += m_listOfItems[i].m_Attack;
            }

            if (m_listOfItems[i]._ItemType == ItemBase.ItemType.Armour)
            {
                mf_ItemDefense += m_listOfItems[i].m_Defense;
            }

            if (m_listOfItems[i]._ItemType == ItemBase.ItemType.Body)
            {
                mf_ItemHealth += m_listOfItems[i].m_Health;
            }

            if (m_listOfItems[i]._ItemType == ItemBase.ItemType.Speed)
            {
                mf_ItemSpeed += m_listOfItems[i].m_Speed;
            }
        }
    }

    public void ProcessSingleItemData(ItemBase ItemPickUp)
    {
        if (ItemPickUp._ItemType == ItemBase.ItemType.Attack)
        {
            mf_ItemMeleeAttack += ItemPickUp.m_Attack;
        }

        if (ItemPickUp._ItemType == ItemBase.ItemType.Armour)
        {
            mf_ItemDefense += ItemPickUp.m_Defense;
        }

        if (ItemPickUp._ItemType == ItemBase.ItemType.Body)
        {
            mf_ItemHealth += ItemPickUp.m_Health;
        }

        if (ItemPickUp._ItemType == ItemBase.ItemType.Speed)
        {
            mf_ItemSpeed += ItemPickUp.m_Speed;
        }
    }

    void UpdateData()
    {
        //TO DO: Update data to Car Controller and other areas where pretinent
    }
}
