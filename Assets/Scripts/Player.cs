using Normal.Realtime;
using Normal.Realtime.Serialization;
using UnityEngine;

public class Player : RealtimeComponent<PlayerModel>
{
    public int _id;
    public string playerName;
    public float playerHealth;
    public float maxPlayerHealth;
    public Vector3 explosionForce;
    private PlayerModel _model;
    protected override void OnRealtimeModelReplaced(PlayerModel previousModel, PlayerModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.playerNameDidChange -= PlayerNameChanged;
            previousModel.healthDidChange -= PlayerHealthChanged;
            previousModel.forcesDidChange -= PlayerForcesChanged;
        }
        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            // use [ if (currentModel.isFreshModel)] to initialize player prefab
            playerName = currentModel.playerName;
            currentModel.playerNameDidChange += PlayerNameChanged;
            currentModel.healthDidChange += PlayerHealthChanged;
            currentModel.forcesDidChange += PlayerForcesChanged;
            _model = currentModel;
        }
    }

    public void SetPlayerName(string _name)
    {
        if (_name.Length > 0)
        {
            _model.playerName = _name;
        }
    }

    public void ChangeExplosionForce(Vector3 _origin)
    {
        _model.forces = _origin;
    }

    public void DamagePlayer(float damage)
    {
        _model.health -= damage;
        //PlayerHealthChanged(_model, (playerHealth - damage));
    }

    public void HealPlayer(float healingPower)
    {
        _model.health += healingPower;
        //PlayerHealthChanged(_model, (playerHealth + healingPower));
    }

    private void PlayerHealthChanged(PlayerModel model, float value)
    {
        playerHealth = value;
    }

    private void PlayerForcesChanged(PlayerModel model, Vector3 value)
    {
        explosionForce = value;
    }

    private void PlayerNameChanged(PlayerModel model, string value)
    {
        playerName = value;
    }
}
