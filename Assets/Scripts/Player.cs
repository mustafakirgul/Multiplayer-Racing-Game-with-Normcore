using Normal.Realtime;

public class Player : RealtimeComponent<PlayerModel>
{
    public string playerName;
    public int playerID;
    private PlayerModel _model;
    protected override void OnRealtimeModelReplaced(PlayerModel previousModel, PlayerModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.playerNameDidChange -= PlayerNameChanged;
        }
        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            // use [ if (currentModel.isFreshModel)] to initialize player prefab

            _model = currentModel;
            currentModel.playerNameDidChange += PlayerNameChanged;
        }
    }

    public void SetPlayerName(string _name)
    {
        if (_name.Length > 0)
        {
            _model.playerName = _name;
        }
    }

    private void PlayerNameChanged(PlayerModel model, string value)
    {
        if (model.isFreshModel)
        {
            playerName = value;
        }
    }
}
