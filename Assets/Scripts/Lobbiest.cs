using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Lobbiest : RealtimeComponent<LobbiestModel>
{
    public Lobbiest(string roomName, int maxPlayers, bool isReady)
    {
        this.roomName = roomName;
        this.maxPlayers = maxPlayers;
        this.isReady = isReady;
    }

    public string roomName;
    public int maxPlayers;
    public bool isReady;

    protected override void OnRealtimeModelReplaced(LobbiestModel previousModel, LobbiestModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.roomNameDidChange -= RoomNameChanged;
            previousModel.maxPlayersDidChange -= MaxPlayersChanged;
            previousModel.isReadyDidChange -= IsReadyChanged;
        }

        if (currentModel != null)
        {
            currentModel.roomNameDidChange += RoomNameChanged;
            currentModel.maxPlayersDidChange += MaxPlayersChanged;
            currentModel.isReadyDidChange += IsReadyChanged;
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);
        RegisterLobbiest();
    }

    private void OnDestroy()
    {
        LobbyManager.instance.RemoveLobbiest(this);
    }

    public void RegisterLobbiest()
    {
        LobbyManager.instance.RegisterLobbiest(this);
    }

    public void ChangeRoomName(string value)
    {
        model.roomName = value;
    }

    public void ChangeMaxPlayers(int value)
    {
        model.maxPlayers = value;
    }

    public void ChangeIsReady(bool value)
    {
        model.isReady = value;
    }


    private void RoomNameChanged(LobbiestModel model, string value)
    {
        roomName = value;
    }

    private void MaxPlayersChanged(LobbiestModel model, float value)
    {
        maxPlayers = Convert.ToInt32(value);
    }

    private void IsReadyChanged(LobbiestModel model, bool value)
    {
        isReady = value;
    }
}