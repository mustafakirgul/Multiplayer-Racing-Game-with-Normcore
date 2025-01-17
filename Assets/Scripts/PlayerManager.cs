﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

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

    PrespawnManager prespawnManager;
    public Vector3 spawnCenter;
    public float directionOffset;
    public float diameter;
    public float maxFanAngle;
    public int spawnPointCount = 9;
    public float startHeight;
    public Vector3[] spawnPoints;
    public LayerMask groundLayer;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireMesh(gizmoMesh, spawnPoint, Quaternion.Euler(-90, spawnRotation, 0), new Vector3(293, 539, 293));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnCenter, 1f);
        GenerateSpawnPoints();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(spawnCenter, spawnPoints[i]);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(spawnPoints[i], 1f);
        }
    }

    public void GenerateSpawnPoints()
    {
        spawnPoints = new Vector3[spawnPointCount];
        var diff = maxFanAngle / spawnPointCount;
        var start = directionOffset - (maxFanAngle * .5f);
        for (int i = 0; i < spawnPointCount; i++)
        {
            spawnPoints[i] = spawnCenter +
                             (Quaternion.AngleAxis(start + (diff * i), Vector3.up) * (Vector3.forward * diameter));
            Physics.Raycast(spawnPoints[i], Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer);
            spawnPoints[i] = hit.point + (Vector3.up * startHeight);
        }
    }

    public Vector3 GetSpawnPoint(int index)
    {
        index %= spawnPoints.Length;
        return spawnPoints[index];
    }

    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    #endregion

    public List<Transform> networkPlayers;

    public Transform localPlayer;
    GameObject _temp;
    public Realtime _realtime;
    public PlayerInfo[] allPlayers;

    private void Awake()
    {
        SingletonCheck();
        wait = new WaitForSeconds(20f);
        _realtime = FindObjectOfType<Realtime>();
        prespawnManager = FindObjectOfType<PrespawnManager>();
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
        allPlayers = new PlayerInfo[carControllers.Length];
        foundTransforms = new Transform[carControllers.Length];
        networkPlayers = new List<Transform>();
        for (int i = 0; i < carControllers.Length; i++)
        {
            foundTransforms[i] = carControllers[i].transform;
            if (carControllers[i]._realtimeView.isOwnedRemotelyInHierarchy)
            {
                if (!networkPlayers.Contains(foundTransforms[i]))
                {
                    networkPlayers.Add(foundTransforms[i]);
                }
            }

            allPlayers[i] = new PlayerInfo(carControllers[i].GetComponent<Player>(),
                carControllers[i]._realtimeView.isOwnedLocallyInHierarchy);
        }
    }

    public string PlayerName(int id)
    {
        string temp = id.ToString(); // if cannot find player id in the list return the id as the name
        if (id < allPlayers.Length)
        {
            foreach (PlayerInfo p in allPlayers)
            {
                if (p.id == id) temp = p.name;
            }
        }

        return temp;
    }

    public Player ReturnPlayer(int id)
    {
        if (id < allPlayers.Length)
        {
            foreach (PlayerInfo p in allPlayers)
            {
                if (p.id == id) return p.player;
            }
        }

        return null;
    }

    public void AddLocalPlayer(Transform _player)
    {
        localPlayer = _player;
        if (cR_playerListCleanUp != null) StopCoroutine(cR_playerListCleanUp);
        cR_playerListCleanUp = StartCoroutine(CR_PlayerListCleanUp());
        //this is set as error so that it shows up in the debug file of build
        //Debug.LogWarning("Local player set as " + (GameManager.instance.isHost ? "host" : "guest"));
        if (GameManager.instance.isHost)
        {
            // Truck truck = FindObjectOfType<Truck>();
            // if (truck != null)
            // {
            //     truck.realtimeView.RequestOwnership();
            //     truck.rtTransform.RequestOwnership();
            //     Realtime.Destroy(truck.gameObject);
            // }

            _temp = Realtime.Instantiate("WeirdTruck",
                position: spawnPoint,
                rotation: Quaternion.Euler(0, spawnRotation, 0),
                ownedByClient: true,
                preventOwnershipTakeover: true,
                destroyWhenOwnerOrLastClientLeaves: true,
                useInstance: _realtime);
            GameManager.instance.RecordRIGO(_temp);
            _temp.GetComponent<Truck>().StartHealth();
            if (prespawnManager != null)
                prespawnManager._realtime = _realtime;
            SpawnItems();
        }
    }

    public void SpawnItems()
    {
        prespawnManager.ReActivateSpawner();
        prespawnManager.SpawnPredeterminedItems();
        prespawnManager.DeActivateSpawner();
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

[Serializable]
public struct PlayerInfo
{
    public int id;
    public string name;
    public bool isLocal;
    public Player player;

    public PlayerInfo(Player player, bool isLocal)
    {
        this.player = player;
        id = player.GetComponent<RealtimeView>().ownerIDInHierarchy;
        name = player.playerName;
        this.isLocal = isLocal;
    }
}