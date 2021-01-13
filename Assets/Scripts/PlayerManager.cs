using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using UnityEngine.Analytics;

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

    public List<Transform> totalPlayers;

    public Transform localPlayer;
    public GameObject pointer;

    public int ownerIDToAssign = -1;

    GameObject _temp;
    List<Transform> _pointers;
    Realtime _realtime;
    private void Awake()
    {
        SingletonCheck();
        _realtime = FindObjectOfType<Realtime>();
        _pointers = new List<Transform>();
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
        _player.GetComponent<NewCarController>().this_ownerID = ownerIDToAssign++;

        if (FindObjectOfType<Truck>() == null)
        {
            Realtime.Instantiate("WeirdTruck",
                    position: localPlayer.position - Vector3.right * 30f,
                    rotation: Quaternion.identity,
               ownedByClient: true,
    preventOwnershipTakeover: false,
    destroyWhenOwnerOrLastClientLeaves: false,
                 useInstance: _realtime);
        }
        else if (FindObjectOfType<Truck>().GetComponent<RealtimeTransform>().isUnownedSelf)
        {
            _temp = FindObjectOfType<Truck>().gameObject;
            _temp.GetComponent<RealtimeView>().RequestOwnership();
            _temp.GetComponent<RealtimeTransform>().RequestOwnership();
            _temp.transform.position = localPlayer.position - Vector3.right * 30f;
            _temp.transform.rotation = Quaternion.identity;
        }
        for (int i = 0; i < networkPlayers.Count; i++)
        {
            _temp = Realtime.Instantiate("Pointer",
                    position: localPlayer.position,
                    rotation: Quaternion.identity,
               ownedByClient: true,
    preventOwnershipTakeover: true,
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
            AssignPlayerID(_player);

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

    public void AssignPlayerID(Transform _player)
    {
        if (_player.GetComponent<NewCarController>() != null)
        {
            ownerIDToAssign++;
            _player.GetComponent<NewCarController>().this_ownerID = ownerIDToAssign;
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
