using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class PrespawnManager : MonoBehaviour
{
    [SerializeField] List<GameObject> PrespawnedItems = new List<GameObject>();

    // Start is called before the first frame update
    Realtime _realtime;

    void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        ReferenceItemsToSpawn();
    }

    private void ReferenceItemsToSpawn()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            PrespawnedItems.Add(this.transform.GetChild(i).gameObject);
        }
    }

    public void SpawnPredeterminedItems()
    {
        foreach (GameObject PowerUp in PrespawnedItems)
        {
            SpawnItemInGameWorld(PowerUp.transform, PowerUp.GetComponent<Loot>().id);
        }
    }

    private void SpawnItemInGameWorld(Transform lootLocation, int lootID)
    {
        {
            GameObject _temp = Realtime.Instantiate("PredeterminedDrop",
                position: lootLocation.position,
                rotation: Quaternion.identity,
                ownedByClient:
                false,
                preventOwnershipTakeover:
                true,
                useInstance:
                _realtime);
            _temp.GetComponent<RealtimeView>().SetOwnership(_realtime.clientID);
            _temp.GetComponent<RealtimeTransform>().SetOwnership(_realtime.clientID);
            _temp.GetComponent<LootContainer>().SetID(lootID);
        }
    }

    public void DeActivateSpawner()
    {
        this.gameObject.SetActive(false);
    }

    public void ReActivateSpawner()
    {
        this.gameObject.SetActive(true);
    }
}