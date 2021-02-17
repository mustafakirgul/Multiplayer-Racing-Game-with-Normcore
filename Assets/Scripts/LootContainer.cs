using System;
using System.Collections;
using Normal.Realtime;
using UnityEngine;

public class LootContainer : MonoBehaviour
{
    private Loot content => GetComponent<Loot>();
    public GameObject loot, pickup;
    public int id, collectedBy;
    public float dieDelay = 1f;
    private RealtimeView _realtime => GetComponent<RealtimeView>();
    private Transform selection;
    private bool customMesh;
    private ParticleSystem collectionParticle;

    [SerializeField] private bool isNetworkInstance;
    private Coroutine cr_Die;
    private Camera mainCamera;

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
        mainCamera = Camera.main;
        collectionParticle = transform.GetChild(2).GetChild(0).GetComponent<ParticleSystem>();
        isNetworkInstance = !_realtime.isOwnedLocallyInHierarchy;
        if (isNetworkInstance)
            GetComponent<Rigidbody>().isKinematic = true;
        customMesh = GetComponent<PowerUpMeshGetter>() != null;
    }

    private void Update()
    {
        if (content == null) return;
        if (selection == null || !customMesh) return;
        selection.localEulerAngles = new Vector3(0, selection.localEulerAngles.y + (180 * Time.deltaTime), 0);
    }

    public int SetID(int _id)
    {
        content.SetID(_id);
        id = _id;
        if (loot == null)
            loot = transform.GetChild(0).gameObject;
        if (pickup == null)
            pickup = transform.GetChild(1).gameObject;

        loot.SetActive(id > 0);
        pickup.SetActive(id < 0);

        selection = loot.activeInHierarchy ? loot.transform : pickup.transform;
        return _id;
    }

    private void SetCollectedBy(int _collectedBy)
    {
        collectedBy = _collectedBy;
        content.SetCollectedBy(_collectedBy);
    }

    public void GetCollected(int _collectorID)
    {
        if (cr_Die == null && content.collectedBy < 0)
        {
            SetCollectedBy(_collectorID);
            cr_Die = StartCoroutine(CR_Die());
        }
    }
    public void DisplayCollectionMessage()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<BoxCollider>().enabled = false;
        if (mainCamera != null)
            collectionParticle.transform.LookAt(mainCamera.transform);
        collectionParticle.Play();
        //Logic for power up vs. loot added
        string tempName;

        if (content.id < 0)
        {
            tempName = "POWERUP (" + LootManager.instance.playerLootPoolSave
                .PlayerPowerUps[Mathf.Abs(content.id) - 1].name
                .Remove(0, 2) + ") !";
        }
        else
        {
            tempName = "Loot Obained!! ";
        }

        GameManager.instance.uIManager.DisplayUIMessage(PlayerManager.instance.PlayerName(collectedBy)
                                                        +
                                                        " has collected a " + (id > 0 ? "loot!" : tempName));
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }
    }

    public IEnumerator CR_Die()
    {
        yield return new WaitForSeconds(dieDelay);
        if (!isNetworkInstance) Realtime.Destroy(gameObject);
    }
}