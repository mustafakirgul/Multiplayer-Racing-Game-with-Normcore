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

    private void Update()
    {
        if (model == null)
            return;
        if (model.playerName != playerName)
        {
            if (model.playerName.Length > 0)
            {
                playerName = model.playerName;
                if (controller == null) return;
                controller.IDDisplay.SetText(playerName);
            }
        }
    }

    public void SetPlayerName(string _name)
    {
        model.playerName = _name;
    }

    public void ResetHealth()
    {
        model.health = maxPlayerHealth;
    }

    public void ChangeExplosionForce(Vector3 _origin)
    {
        model.forces = _origin;
    }

    public void DamagePlayer(float damage)
    {
        model.health -= ((1 - armourDefenseModifier) * damage);

        if (controller != null)
        {
            controller.DamageFeedback();
        }
    }

    public void HealPlayer(float healingPower)
    {
        model.health += ((1 + healModifier) * healingPower);
    }

    private void PlayerHealthChanged(PlayerModel playerModel, float value)
    {
        playerHealth = model.health;
    }

    private void PlayerForcesChanged(PlayerModel playerModel, Vector3 value)
    {
        explosionForce = model.forces;
    }

    private void PlayerNameChanged(PlayerModel playerModel, string value)
    {
        playerName = model.playerName;
    }
}