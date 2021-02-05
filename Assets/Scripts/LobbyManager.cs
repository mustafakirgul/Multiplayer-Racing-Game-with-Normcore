using System;
using System.Collections;
using System.Collections.Generic;
using Normal.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    private Lobbiest _lobbiest;
    private Realtime _realtime => FindObjectOfType<Realtime>();
    private Coroutine cr_RoomChecker;

    private bool isConnectedToALobby;

    //public float serverCheckDelay;
    public List<Lobbiest> lobbiests;

    //private WaitForSeconds wait => new WaitForSeconds(serverCheckDelay);
    public Image radialLoader;
    public Text playerNumber, readyPlayerNumber;
    private string roomName;
    private UIManager uIManager;

    private void Awake()
    {
        _realtime.didConnectToRoom += DidConnectToLobby;
        _realtime.didDisconnectFromRoom += DidDisconnectFromLobby;
        lobbiests = new List<Lobbiest>();
        uIManager = FindObjectOfType<UIManager>();
    }

    private void Start()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            instance = this;
        }

        //----------------------------This part will change after V2
        ConnectToLobby("UGP_TEST5");
    }

    private void Update()
    {
        if (isConnectedToALobby)
        {
            AnimateUI();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void AnimateUI()
    {
        int count = lobbiests.Count;
        playerNumber.text = count.ToString();
        int ready = 0;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (lobbiests[i].isReady) ready++;
            }

            radialLoader.fillAmount = (ready * 1f) / (count * 1f);
            if (count == ready)
                uIManager.ConnectToRoom();
        }
        else
        {
            radialLoader.fillAmount = 0f;
        }

        readyPlayerNumber.text = ready.ToString();
    }

    // public void CreateRoom(string roomName)
    // {
    //     CheckRoom(roomName);
    //     //if it does not exist, create it
    // }

    public void ConnectToLobby(string roomName)
    {
        this.roomName = roomName;
        _realtime.Connect(roomName + "_L088Y");
    }

    public void DisconnectFromLobby()
    {
        if (_realtime.connected)
        {
            _realtime.Disconnect();
        }
    }

    public void MarkPlayerReady()
    {
        if (_lobbiest == null) return;
        _lobbiest.ChangeIsReady(true);
    }

    // void CheckRoom(string roomName)
    // {
    //     if (cr_RoomChecker == null)
    //     {
    //         cr_RoomChecker = StartCoroutine(CR_CheckRoom(roomName));
    //     }
    // }
    //
    // IEnumerator CR_CheckRoom(string roomName)
    // {
    //     isConnectedToALobby = false;
    //     _realtime.Connect(roomName + "_L088Y");
    //     while (!isConnectedToALobby)
    //     {
    //         //wait
    //     }
    //
    //     yield return wait;
    // }

    public void RegisterLobbiest(Lobbiest lobbiest)
    {
        if (lobbiests.Contains(lobbiest)) return;
        lobbiests.Add(lobbiest);
    }

    public void RemoveLobbiest(Lobbiest lobbiest)
    {
        if (lobbiests.Contains(lobbiest))
            lobbiests.Remove(lobbiest);
    }

    void DidConnectToLobby(Realtime realtime)
    {
        isConnectedToALobby = true;
        GameObject _temp = Realtime.Instantiate("Lobbiest",
            position: Vector3.zero,
            rotation: Quaternion.identity,
            ownedByClient: true,
            preventOwnershipTakeover: true,
            useInstance: _realtime);
        _lobbiest = _temp.GetComponent<Lobbiest>();
        _lobbiest.ChangeRoomName(_realtime.room.name);
        _lobbiest.ChangeMaxPlayers(8);
        Debug.LogWarning("Connected to lobby.");
    }

    void DidDisconnectFromLobby(Realtime realtime)
    {
        isConnectedToALobby = false;
        lobbiests.Clear();
        _realtime.didConnectToRoom -= DidConnectToLobby;
        _realtime.didDisconnectFromRoom -= DidDisconnectFromLobby;
    }
}