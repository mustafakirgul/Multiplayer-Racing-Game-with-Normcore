using System;
using System.Collections;
using System.Collections.Generic;
using Normal.Realtime;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    private Lobby lobby;
    private Realtime _realtime => FindObjectOfType<Realtime>();
    private Coroutine cr_RoomChecker;
    private bool isConnectedToALobby;
    public float serverCheckDelay;
    private WaitForSeconds wait => new WaitForSeconds(serverCheckDelay);

    private void Awake()
    {
        _realtime.didConnectToRoom += DidConnectToLobby;
        _realtime.didDisconnectFromRoom += DidDisconnectFromLobby;
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
    }

    public void CreateRoom(string roomName)
    {
        CheckRoom(roomName);
    }

    void CheckRoom(string roomName)
    {
        if (cr_RoomChecker == null)
        {
            cr_RoomChecker = StartCoroutine(CR_CheckRoom(roomName));
        }
    }

    IEnumerator CR_CheckRoom(string roomName)
    {
        isConnectedToALobby = false;
        _realtime.Connect(roomName);
        while (!isConnectedToALobby)
        {
            //wait
        }

        yield return wait;
        
        
    }

    void DidConnectToLobby(Realtime realtime)
    {
        isConnectedToALobby = true;
    }

    void DidDisconnectFromLobby(Realtime realtime)
    {
        isConnectedToALobby = false;
    }
}