﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using UnityEngine.Analytics;

public class PlayerManager : MonoBehaviour
{
    #region Singleton Logic

    public static PlayerManager instance = null;

    private Coroutine cR_playerListCleanUp;
    private WaitForSeconds wait;

    private Transform[] foundTransforms;
    private NewCarController[] carControllers;

    public Vector3 spawnPoint;
    public Mesh gizmoMesh;
    [Range(0, 359)] public float spawnRotation;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireMesh(gizmoMesh, spawnPoint, Quaternion.Euler(-90, spawnRotation, 0), new Vector3(293, 539, 293));
    }

    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
    }

    #endregion

    public List<Transform> networkPlayers;

    public Transform localPlayer;

    GameObject _temp;
    public Realtime _realtime;

    private void Awake()
    {
        SingletonCheck();
        wait = new WaitForSeconds(20f);
        cR_playerListCleanUp = StartCoroutine(CR_PlayerListCleanUp());
        _realtime = FindObjectOfType<Realtime>();
        //GameManager.instance.PlayerCountDownCheck();
    }

    IEnumerator CR_PlayerListCleanUp()
    {
        while (true)
        {
            UpdateExistingPlayers();
            yield return wait;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void UpdateExistingPlayers()
    {
        carControllers = FindObjectsOfType<NewCarController>();
        foundTransforms = new Transform[carControllers.Length];
        networkPlayers = new List<Transform>();
        for (int i = 0; i < carControllers.Length; i++)
        {
            foundTransforms[i] = carControllers[i].transform;
            if (carControllers[i].isNetworkInstance)
            {
                if (!networkPlayers.Contains(foundTransforms[i]))
                {
                    networkPlayers.Add(foundTransforms[i]);
                }
            }
        }
    }

    public int RequestOwner()
    {
        return _realtime.clientID;
    }

    public void AddLocalPlayer(Transform _player)
    {
        localPlayer = _player;
        UpdateExistingPlayers();
        Truck truck = FindObjectOfType<Truck>();
        if (truck == null)
        {
            _temp = Realtime.Instantiate("WeirdTruck",
                position: spawnPoint,
                rotation: Quaternion.Euler(0, spawnRotation, 0),
                ownedByClient: true,
                preventOwnershipTakeover: false,
                destroyWhenOwnerOrLastClientLeaves: true,
                useInstance: _realtime);
            _temp.GetComponent<Truck>().StartHealth();
        }
        else if (truck.GetComponent<RealtimeTransform>().isUnownedInHierarchy)
        {
            _temp = FindObjectOfType<Truck>().gameObject;
            _temp.GetComponent<RealtimeView>().RequestOwnership();
            _temp.GetComponent<RealtimeTransform>().RequestOwnership();
            _temp.GetComponent<Truck>().SetOwner(localPlayer.GetComponent<Player>()._id);
            _temp.transform.position = spawnPoint;
            _temp.transform.rotation = Quaternion.Euler(0, spawnRotation, 0);
            _temp.GetComponent<Truck>().StartHealth();
        }
        else
        {
            truck.StartHealth();
        }
    }

    public void AddNetworkPlayer(Transform _player)
    {
        if (!networkPlayers.Contains(_player))
        {
            networkPlayers.Add(_player);
        }
    }

    public void RemoveNetworkPlayer(Transform _player)
    {
        if (networkPlayers.Contains(_player))
        {
            networkPlayers.Remove(_player);
        }
    }

    public void CleanEmptiesInLists()
    {
        networkPlayers.Clear();
        //connectedPlayers.Clear();

        //for (int i = networkPlayers.Count - 1; i > 0; i--)
        //{
        //    if(networkPlayers[i] == null)
        //    {
        //        networkPlayers.RemoveAt(i);
        //    }
        //}

        //for (int i = connectedPlayers.Count - 1; i > 0; i--)
        //{
        //    if (networkPlayers[i] == null)
        //    {
        //        connectedPlayers.RemoveAt(i);
        //    }
        //}

        localPlayer = null;
    }
}