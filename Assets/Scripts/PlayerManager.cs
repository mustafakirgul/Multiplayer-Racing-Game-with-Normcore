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
    List<Transform> _pointers;
    Realtime _realtime;
    private void Awake()
    {
        SingletonCheck();
        _realtime = FindObjectOfType<Realtime>();
        _pointers = new List<Transform>();
    }
    public void AddLocalPlayer(Transform _player)
    {
        localPlayer = _player;
        if (FindObjectOfType<Truck>() == null)
        {
            Realtime.Instantiate("WeirdTruck",
                    position: localPlayer.position + -transform.right * 8f,
                    rotation: Quaternion.identity,
               ownedByClient: true,
    preventOwnershipTakeover: false,
                 useInstance: _realtime);
        }
        for (int i = 0; i < networkPlayers.Count; i++)
        {
            _temp = Realtime.Instantiate("Pointer",
                    position: localPlayer.position,
                    rotation: Quaternion.identity,
               ownedByClient: true,
    preventOwnershipTakeover: false,
                 useInstance: _realtime);
            //_temp.GetComponent<RealtimeView>().RequestOwnership();
            //_temp.GetComponent<RealtimeTransform>().RequestOwnership();
            _temp.GetComponent<Pointer>().Initialize(localPlayer, networkPlayers[i]);
            _temp.transform.parent = localPlayer;
            _pointers.Add(_temp.transform);
        }
    }

    public void AddNetworkPlayer(Transform _player)
    {
        if (!networkPlayers.Contains(_player))
        {
            networkPlayers.Add(_player);

            if (localPlayer != null)
            {
                _temp = Realtime.Instantiate("Pointer",
                        position: localPlayer.position,
                        rotation: Quaternion.identity,
                   ownedByClient: true,
        preventOwnershipTakeover: true,
                     useInstance: _realtime);
                _temp.GetComponent<Pointer>().Initialize(localPlayer, _player);
                _temp.transform.parent = localPlayer;
                _pointers.Add(_temp.transform);
            }
        }
    }

    public void RemovePointer(Transform _pointer)
    {
        if (_pointers.Contains(_pointer))
        {
            _pointers.Remove(_pointer);
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
