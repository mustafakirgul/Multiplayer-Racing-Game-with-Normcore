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
            if (currentModel.isFreshModel)
            {
                playerName = model.playerName;
            }

            currentModel.playerNameDidChange += PlayerNameChanged;
            currentModel.healthDidChange += PlayerHealthChanged;
            currentModel.forcesDidChange += PlayerForcesChanged;
        }
    }

    private void Start()
    {
        _id = GetComponent<RealtimeView>().ownerIDSelf;
        SetHealth(maxPlayerHealth);
    }

    public void SetPlayerName(string _name)
    {
        model.playerName = _name;
    }

    public void ChangeExplosionForce(Vector3 _origin)
    {
        model.forces = _origin;
    }

    public void DamagePlayer(float damage)
    {
        model.health -= ((1 - armourDefenseModifier) * damage);
        Debug.LogWarning("Player received " + damage + " damage. | Calculated damage: " +
                         ((1 - armourDefenseModifier) * damage));
    }

    public void SetHealth(float value)
    {
        Debug.LogWarning("Model health set to: " + value);
        model.health = value;
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