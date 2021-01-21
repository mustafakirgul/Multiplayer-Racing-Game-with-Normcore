using System;
using System.Collections;
using Normal.Realtime;
using UnityEngine;

public class LootContainer : MonoBehaviour
{
    private Loot content => GetComponent<Loot>();
    public GameObject loot, pickup;
    public int id, collectedBy;
    public float dieDelay = .5f;
    private RealtimeView _realtime => GetComponent<RealtimeView>();
    private bool isNetworkInstance;

    private void OnDrawGizmos()
    {
        if (loot == null || pickup == null) return;
        if (!loot.activeInHierarchy && !pickup.activeInHierarchy)
        {
            Gizmos.DrawCube(transform.position, new Vector3(2.2f, 2.2f, 2.2f));
        }
    }

    private void Start()
    {
        isNetworkInstance = !_realtime.isOwnedLocallySelf;
        if (isNetworkInstance)
            GetComponent<Rigidbody>().isKinematic = true;
        SetID(content.id);
    }

    public int SetID(int _id)
    {
        id = _id;
        if (loot == null)
            loot = transform.GetChild(0).gameObject;
        if (pickup == null)
            pickup = transform.GetChild(1).gameObject;
        loot.SetActive(id > 0);
        pickup.SetActive(id < 0);
        return _id;
    }

    private int SetCollectedBy(int _collectedBy)
    {
        collectedBy = content.SetCollectedBy(_collectedBy);
        return collectedBy;
    }

    public int GetCollected(int _collectorID)
    {
        if (content.collectedBy == 0)
        {
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(CR_Die());
            SetCollectedBy(_collectorID);
            return id; // return id of collected item
        }

        return
            0; //return 0 meaning the item was already collected by someone and pending to be destroyed from the game world
    }

    IEnumerator CR_Die()
    {
        yield return new WaitForSeconds(dieDelay);
        Realtime.Destroy(gameObject);
    }
}