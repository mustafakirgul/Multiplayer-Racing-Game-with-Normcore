using System;
using Normal.Realtime;
using UnityEngine;

public class Lobbiest : RealtimeComponent<LobbiestModel>
{
    public string roomName;
    public int maxPlayers;
    public bool isReady;
    public bool isHost;

    protected override void OnRealtimeModelReplaced(LobbiestModel previousModel, LobbiestModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);
        if (previousModel != null)
        {
            previousModel.roomNameDidChange -= RoomNameChanged;
            previousModel.maxPlayersDidChange -= MaxPlayersChanged;
            previousModel.isReadyDidChange -= IsReadyChanged;
            previousModel.isHostDidChange -= IsHostChanged;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                roomName = currentModel.roomName;
                maxPlayers = Convert.ToInt32(currentModel.maxPlayers);
                isHost = currentModel.isHost;
                GameManager.instance.isHost = isHost;
            }

            currentModel.roomNameDidChange += RoomNameChanged;
            currentModel.maxPlayersDidChange += MaxPlayersChanged;
            currentModel.isReadyDidChange += IsReadyChanged;
            currentModel.isHostDidChange += IsHostChanged;
        }
    }

    public void UpdateLobbiest()
    {
        roomName = model.roomName;
        maxPlayers = Convert.ToInt32(model.maxPlayers);
        isHost = model.isHost;
        GameManager.instance.isHost = isHost;
        isReady = model.isReady;
    }

    private void Start()
    {
        LobbyManager.instance.RegisterLobbiest(this);
    }

    private void OnDestroy()
    {
        LobbyManager.instance.RemoveLobbiest(this);
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

    public void ChangeIsHost(bool value)
    {
        model.isHost = value;
    }

    private void IsHostChanged(LobbiestModel lobbiestModel, bool value)
    {
        Debug.LogWarning("IsHost changed to " + value);
        isHost = value;
        GameManager.instance.isHost = value;
    }

    private void RoomNameChanged(LobbiestModel lobbiestModel, string value)
    {
        roomName = value;
    }

    private void MaxPlayersChanged(LobbiestModel lobbiestModel, float value)
    {
        maxPlayers = Convert.ToInt32(value);
    }

    private void IsReadyChanged(LobbiestModel lobbiestModel, bool value)
    {
        isReady = value;
    }
}