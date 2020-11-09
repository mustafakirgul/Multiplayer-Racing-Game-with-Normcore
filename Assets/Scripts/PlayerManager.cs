using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class PlayerManager : MonoBehaviour
{
    #region Singleton Logic
    public static PlayerManager instance = null;
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
    public GameObject pointer;

    GameObject _temp;
    Realtime _realtime;
    bool _localPlayerOwnsTruck;
    private void Awake()
    {
        SingletonCheck();
        _realtime = FindObjectOfType<Realtime>();
    }
    public void AddLocalPlayer(Transform _player)
    {
        localPlayer = _player;
        if (FindObjectOfType<Truck>() == null)
        {
            Realtime.Instantiate("Truck",
                    position: new Vector3(0, 25, 0),
                    rotation: Quaternion.identity,
               ownedByClient: true,
    preventOwnershipTakeover: false,
                 useInstance: _realtime);
            _localPlayerOwnsTruck = true;
        }
        for (int i = 0; i < networkPlayers.Count; i++)
        {
            _temp = Realtime.Instantiate("Pointer",
                    position: localPlayer.position,
                    rotation: Quaternion.identity,
               ownedByClient: true,
    preventOwnershipTakeover: true,
                 useInstance: _realtime);
            _temp.GetComponent<Pointer>().Initialize(localPlayer, networkPlayers[i]);
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
}
