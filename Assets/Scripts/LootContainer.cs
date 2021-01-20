using UnityEngine;

public class LootContainer : MonoBehaviour
{
    private Loot content => GetComponent<Loot>();
    public GameObject loot, pickup;
    public int id, collectedBy;

    private void OnDrawGizmos()
    {
        if (loot == null || pickup == null) return;
        if (!loot.activeInHierarchy && !pickup.activeInHierarchy)
        {
            Gizmos.DrawCube(transform.position, new Vector3(2.2f, 2.2f, 2.2f));
        }
    }

    public int UpdateID(int _id)
    {
        id = content.SetID(_id);
        if (loot == null)
            loot = transform.GetChild(0).gameObject;
        if (pickup == null)
            pickup = transform.GetChild(1).gameObject;
        loot.SetActive(id > 0);
        pickup.SetActive(id < 0);
        return id;
    }

    public int UpdateCollectedBy(int _collectedBy)
    {
        collectedBy = content.SetCollectedBy(_collectedBy);
        return collectedBy;
    }
}