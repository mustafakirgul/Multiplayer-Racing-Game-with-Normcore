using System;
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
    public PlayerInfo[] allPlayers;

    private void Awake()
    {
        SingletonCheck();
        wait = new WaitForSeconds(20f);
        _realtime = FindObjectOfType<Realtime>();
        prespawnManager = FindObjectOfType<PrespawnManager>();
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
        allPlayers = new PlayerInfo[carControllers.Length];
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

            allPlayers[i] = new PlayerInfo(carControllers[i].GetComponent<Player>(),
                carControllers[i].isNetworkInstance);
        }
    }

    public int RequestOwner()
    {
        int _lowestID = _realtime.clientID;
        foreach (Transform player in networkPlayers)
        {
            int _temp = player.GetComponent<NewCarController>()._player._id;
            if (_temp < _lowestID)
            {
                _lowestID = _temp;
            }
        }

        return _lowestID;
    }

    public string PlayerName(int id)
    {
        string temp = "N/A";
        if (id < allPlayers.Length)
        {
            foreach (PlayerInfo p in allPlayers)
            {
                if (p.id == id) temp = p.name;
            }
        }

        return temp;
    }

    public void AddLocalPlayer(Transform _player)
    {
        localPlayer = _player;
        if (cR_playerListCleanUp != null) StopCoroutine(cR_playerListCleanUp);
        cR_playerListCleanUp = StartCoroutine(CR_PlayerListCleanUp());
        //this is set as error so that it shows up in the debug file of build
        Debug.LogError("Local player set as " + (GameManager.instance.isHost ? "host" : "guest"));
        if (GameManager.instance.isHost)
        {
            Truck truck = FindObjectOfType<Truck>();
            if (truck != null)
            {
                truck.realtimeView.RequestOwnership();
                truck.rtTransform.RequestOwnership();
                Realtime.Destroy(truck.gameObject);
            }

            _temp = Realtime.Instantiate("WeirdTruck",
                position: spawnPoint,
                rotation: Quaternion.Euler(0, spawnRotation, 0),
                ownedByClient: true,
                preventOwnershipTakeover: false,
                destroyWhenOwnerOrLastClientLeaves: true,
                useInstance: _realtime);
            _temp.GetComponent<Truck>().StartHealth();

            prespawnManager._realtime = _realtime;
            prespawnManager.ReActivateSpawner();
            prespawnManager.SpawnPredeterminedItems();
            prespawnManager.DeActivateSpawner();
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

[Serializable]
public struct PlayerInfo
{
    public int id;
    public string name;
    public bool isLocal;

    public PlayerInfo(int id, string name, bool isLocal)
    {
        this.id = id;
        this.name = name;
        this.isLocal = isLocal;
    }

    public PlayerInfo(Player player, bool isLocal)
    {
        id = player._id;
        name = player.playerName;
        this.isLocal = isLocal;
    }
}