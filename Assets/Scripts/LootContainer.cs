﻿using System.Collections;
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

    private Coroutine cr_Die;
    private Camera mainCamera;
    public LootCollectionFeedback lCF => FindObjectOfType<LootCollectionFeedback>();
    public LayerMask groundMask;
    public AudioPlayer sound;


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
        if (_realtime.isOwnedRemotelyInHierarchy)
            GetComponent<Rigidbody>().isKinematic = true;
        customMesh = GetComponent<PowerUpMeshGetter>() != null;
    }

    private void Update()
    {
        if (transform.position.y < -1000f) Realtime.Destroy(gameObject);
        if (content == null) return;
        if (selection == null || !customMesh) return;
        selection.localEulerAngles = new Vector3(0, selection.localEulerAngles.y + (180 * Time.deltaTime), 0);
    }

    public void SetID(int _id)
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
    }

    private void SetCollectedBy(int _collectedBy)
    {
        collectedBy = _collectedBy;
        content.SetCollectedBy(_collectedBy);
    }

    public void GetCollected(int _collectorID)
    {
        if (cr_Die == null)
        {
            if (sound == null) GetComponent<AudioPlayer>();
            SetCollectedBy(_collectorID);
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<BoxCollider>().enabled = false;
            cr_Die = StartCoroutine(CR_Die());
            DisplayCollectionMessage();
            if (content.id < 0)
            {
                PlayerManager.instance.ReturnPlayer(_collectorID).statsEntity.ReceiveStat(StatType.powerup);
                if (sound != null) sound.PlayIndex(1);
            }
            else
            {
                PlayerManager.instance.ReturnPlayer(_collectorID).statsEntity.ReceiveStat(StatType.loot);
                if (sound != null) sound.PlayIndex(0);
            }
        }
    }

    public void DisplayCollectionMessage()
    {
        //Logic for power up vs. loot added
        string tempName;
        if (content.id < 0)
        {
            tempName = LootManager.instance.playerLootPoolSave
                .PlayerPowerUps[Mathf.Abs(content.id) - 1].name
                .Remove(0, 2);
        }
        else
        {
            tempName = "Loot Obtained!! ";
        }

        lCF.PlayAnimation(tempName, content.id);
    }

    public IEnumerator CR_MeshDie()
    {
        GetComponent<Collider>().enabled = false;
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }

        if (mainCamera != null)
            collectionParticle.transform.LookAt(mainCamera.transform);
        collectionParticle.Play();
        yield return null;
    }

    public IEnumerator CR_Die()
    {
        yield return new WaitForSeconds(dieDelay);
        if (_realtime.isOwnedLocallyInHierarchy) Realtime.Destroy(gameObject);
        cr_Die = null;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_realtime.isOwnedLocallyInHierarchy && other.gameObject.layer != groundMask)
        {
            GetComponent<Rigidbody>().AddForce((other.transform.position - transform.position).normalized +
                                               UnityEngine.Random.onUnitSphere * 66.6f);
        }
    }
}