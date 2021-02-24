using System;
using Normal.Realtime;
using UnityEngine;

public class Player : RealtimeComponent<PlayerModel>
{
    public string playerName;
    public float playerHealth;
    public float maxPlayerHealth;
    public Vector3 explosionForce;
    public float armourDefenseModifier = 0f;
    public float tempDefenseModifier = 0f;
    public float healModifier = 0f;
    private NewCarController controller;
    public StatsEntity statsEntity;

    protected override void OnRealtimeModelReplaced(PlayerModel previousModel, PlayerModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);
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
                playerName = GameManager.instance.playerName;
                currentModel.health = maxPlayerHealth;
                currentModel.playerName = playerName;
                playerHealth = maxPlayerHealth;
                ResetHealth();
            }

            currentModel.playerNameDidChange += PlayerNameChanged;
            currentModel.healthDidChange += PlayerHealthChanged;
            currentModel.forcesDidChange += PlayerForcesChanged;
        }
    }

    private void Awake()
    {
        statsEntity = Realtime.Instantiate("StatEntity",
            position: Vector3.zero,
            rotation: Quaternion.identity,
            ownedByClient: true,
            preventOwnershipTakeover: true,
            destroyWhenOwnerOrLastClientLeaves: true,
            useInstance: realtime).GetComponent<StatsEntity>();
    }

    private void Update()
    {
        if (model == null) return;
        playerName = model.playerName;
        playerHealth = model.health;
        controller = GetComponent<NewCarController>();
        if (controller == null) return;
        controller.IDDisplay.SetText(playerName);
        controller._currentName = model.playerName;
    }

    public void ResetHealth()
    {
        model.health = maxPlayerHealth;
    }

    public void UpdateTempDefenseModifier(float value)
    {
        tempDefenseModifier = value;
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
        float calculatedDamage = (1 - armourDefenseModifier - tempDefenseModifier * 0.5f) * damage;
        Debug.LogWarning("Calculated Damage for " + PlayerManager.instance.PlayerName(realtimeView.ownerIDInHierarchy) +
                         " is " + calculatedDamage);
        model.health -= calculatedDamage;

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
        playerHealth = value;
    }

    private void PlayerForcesChanged(PlayerModel playerModel, Vector3 value)
    {
        explosionForce = model.forces;
    }

    private void PlayerNameChanged(PlayerModel playerModel, string value)
    {
        playerName = value;
    }
}