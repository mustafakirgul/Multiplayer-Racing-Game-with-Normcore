using System;
using System.Collections;
using Normal.Realtime;

public class Lobbiest : RealtimeComponent<LobbiestModel>
{
    public string roomName;
    public int maxPlayers;
    public bool isReady;
    public bool isHost;

    protected override void OnRealtimeModelReplaced(LobbiestModel previousModel, LobbiestModel currentModel)
    {
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

    private void Update()
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
        isHost = lobbiestModel.isHost;
        GameManager.instance.isHost = isHost;
    }

    private void RoomNameChanged(LobbiestModel lobbiestModel, string value)
    {
        roomName = lobbiestModel.roomName;
    }

    private void MaxPlayersChanged(LobbiestModel lobbiestModel, float value)
    {
        maxPlayers = Convert.ToInt32(lobbiestModel.maxPlayers);
    }

    private void IsReadyChanged(LobbiestModel lobbiestModel, bool value)
    {
        isReady = lobbiestModel.isReady;
    }
}