using System;
using Normal.Realtime;
using UnityEngine;

public class Player : RealtimeComponent<PlayerModel>
{
    public int _id;
    public string playerName;
    public float playerHealth;
    public float maxPlayerHealth;
    public Vector3 explosionForce;
    public float armourDefenseModifier = 0f;
    public float healModifier = 0f;
    private NewCarController controller;

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
            currentModel.playerNameDidChange += PlayerNameChanged;
            currentModel.healthDidChange += PlayerHealthChanged;
            currentModel.forcesDidChange += PlayerForcesChanged;
        }
    }

    private void Start()
    {
        ResetHealth();
        if (controller == null && !GetComponent<NewCarController>().isNetworkInstance)
        {
            controller = GetComponent<NewCarController>();
            if (!controller.offlineTest)
            {
                playerName = model.playerName;
                _id = GetComponent<RealtimeView>().ownerIDInHierarchy;
            }
        }
    }

    public void SetPlayerName(string _name)
    {
        if (_name.Length > 0)
        {
            model.playerName = _name;
        }
    }

    public void ResetHealth()
    {
        if (model != null)
            model.health = maxPlayerHealth;
    }

    public void ChangeExplosionForce(Vector3 _origin)
    {
        if (model != null)
            model.forces = _origin;
    }

    public void DamagePlayer(float damage)
    {
        if (model != null)
            model.health -= ((1 - armourDefenseModifier) * damage);

        if (controller != null)
        {
            controller.DamageFeedback();
        }
    }

    public void HealPlayer(float healingPower)
    {
        if (model != null)
            model.health += ((1 + healModifier) * healingPower);
    }

    private void PlayerHealthChanged(PlayerModel playerModel, float value)
    {
        if (model != null)
            playerHealth = model.health;
    }

    private void PlayerForcesChanged(PlayerModel playerModel, Vector3 value)
    {
        if (model != null)
            explosionForce = model.forces;
    }

    private void PlayerNameChanged(PlayerModel playerModel, string value)
    {
        if (model != null)
            playerName = model.playerName;
    }
}