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


    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    #endregion

    public List<Transform> networkPlayers;

    public Transform localPlayer;

    GameObject _temp;
    Realtime _realtime;

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

    internal Transform RequestOwner(List<RealtimeTransform> _transforms)
    {
        for (int i = 0; i < _transforms.Count; i++)
        {
            _transforms[i].SetOwnership(_realtime.clientID);
        }

        return localPlayer;
    }

    public void AddLocalPlayer(Transform _player)
    {
        localPlayer = _player;
        UpdateExistingPlayers();
        if (FindObjectOfType<Truck>() == null)
        {
            _temp = Realtime.Instantiate("WeirdTruck",
                position: localPlayer.position + (Vector3.forward * 100f) + (Vector3.up * 5f),
                rotation: Quaternion.identity,
                ownedByClient: true,
                preventOwnershipTakeover: false,
                destroyWhenOwnerOrLastClientLeaves: false,
                useInstance: _realtime);
            _temp.GetComponent<Truck>().StartHealth();
        }
        else if (FindObjectOfType<Truck>().GetComponent<RealtimeTransform>().isUnownedSelf)
        {
            _temp = FindObjectOfType<Truck>().gameObject;
            _temp.GetComponent<RealtimeView>().RequestOwnership();
            _temp.GetComponent<RealtimeTransform>().RequestOwnership();
            _temp.transform.position = localPlayer.position + (Vector3.forward * 100f) + (Vector3.up * 5f);
            _temp.transform.rotation = Quaternion.identity;
            _temp.GetComponent<Truck>().StartHealth();
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