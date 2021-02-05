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
            currentModel.roomNameDidChange += RoomNameChanged;
            currentModel.maxPlayersDidChange += MaxPlayersChanged;
            currentModel.isReadyDidChange += IsReadyChanged;
            currentModel.isHostDidChange += IsHostChanged;
        }
    }


    private IEnumerator Start()
    {
        LobbyManager.instance.RegisterLobbiest(this);
        yield return null;
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
        isHost = value;
        GameManager.instance.isHost = isHost;
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