using System;
using System.Collections;
using Normal.Realtime;
using UnityEngine;

public class Lobbiest : RealtimeComponent<LobbiestModel>
{
    public string roomName;
    public int maxPlayers;
    public bool isReady;
    public bool isHost;
    private RealtimeView _rtView;

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
                Debug.LogError("This machine is the host!");
            }

            UpdateData();
            currentModel.roomNameDidChange += RoomNameChanged;
            currentModel.maxPlayersDidChange += MaxPlayersChanged;
            currentModel.isReadyDidChange += IsReadyChanged;
            currentModel.isHostDidChange += IsHostChanged;
        }
    }

    void UpdateData()
    {
        RoomNameUpdate();
        MaxPlayersUpdate();
        IsReadyUpdate();
        IsHostUpdate();
    }

    private void Start()
    {
        _rtView = GetComponent<RealtimeView>();
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

    private void RoomNameChanged(LobbiestModel lobbiestModel, string value)
    {
        RoomNameUpdate();
    }

    private void RoomNameUpdate()
    {
        roomName = model.roomName;
    }

    public void ChangeMaxPlayers(int value)
    {
        model.maxPlayers = value;
    }

    private void MaxPlayersChanged(LobbiestModel lobbiestModel, float value)
    {
        MaxPlayersUpdate();
    }

    private void MaxPlayersUpdate()
    {
        maxPlayers = Convert.ToInt32(model.maxPlayers);
    }


    public void ChangeIsReady(bool value)
    {
        model.isReady = value;
    }

    private void IsReadyChanged(LobbiestModel lobbiestModel, bool value)
    {
        IsReadyUpdate();
    }

    private void IsReadyUpdate()
    {
        isReady = model.isReady;
    }


    public void ChangeIsHost(bool value)
    {
        model.isHost = value;
        Debug.LogError("Guest: " + !value);
    }

    private void IsHostChanged(LobbiestModel lobbiestModel, bool value)
    {
        IsHostUpdate();
    }

    private void IsHostUpdate()
    {
        isHost = model.isHost;
    }
}