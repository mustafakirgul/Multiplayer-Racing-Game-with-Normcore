using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Lobby : RealtimeComponent<LobbyModel>
{
    public Lobby(string roomName, int maxPlayers, bool isHost)
    {
        this.roomName = roomName;
        this.maxPlayers = maxPlayers;
        this.isHost = isHost;
    }

    public string roomName;
    public int maxPlayers;
    public bool isHost;

    protected override void OnRealtimeModelReplaced(LobbyModel previousModel, LobbyModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.roomNameDidChange -= RoomNameChanged;
            previousModel.maxPlayersDidChange -= MaxPlayersChanged;
            previousModel.isHostDidChange -= IsHostChanged;
        }

        if (currentModel != null)
        {
            currentModel.roomNameDidChange += RoomNameChanged;
            currentModel.maxPlayersDidChange += MaxPlayersChanged;
            currentModel.isHostDidChange += IsHostChanged;
        }
    }

    public void ChangeRoomName(string value)
    {
        model.roomName = value;
    }

    public void ChangeMaxPlayers(int value)
    {
        model.maxPlayers = value;
    }

    public void ChangeIsHost(bool value)
    {
        model.isHost = value;
    }


    private void RoomNameChanged(LobbyModel model, string value)
    {
        roomName = value;
    }

    private void MaxPlayersChanged(LobbyModel model, float value)
    {
        maxPlayers = Convert.ToInt32(value);
    }

    private void IsHostChanged(LobbyModel model, bool value)
    {
        isHost = value;
    }
}