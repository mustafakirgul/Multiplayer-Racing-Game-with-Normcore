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
    public float meleeModifier;
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
            previousModel.isBoostingDidChange -= PlayerIsBoostingChanged;
            previousModel.meleePowerDidChange -= MeleePowerChanged;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                playerName = GameManager.instance.playerName;
                currentModel.health = maxPlayerHealth;
                currentModel.playerName = playerName;
                currentModel.isBoosting = false;
                if (controller != null)
                    currentModel.meleePower = controller.meleeDamageModifier;
                playerHealth = maxPlayerHealth;
                ResetHealth();
            }

            currentModel.playerNameDidChange += PlayerNameChanged;
            currentModel.healthDidChange += PlayerHealthChanged;
            currentModel.forcesDidChange += PlayerForcesChanged;
            currentModel.isBoostingDidChange += PlayerIsBoostingChanged;
        }
    }

    private void MeleePowerChanged(PlayerModel playerModel, float value)
    {
        meleeModifier = value;
        controller.meleeDamageModifier = value;
    }

    public void ChangeIsBoosting(bool value)
    {
        model.isBoosting = value;
        controller.isBoosting = value;
    }

    private void PlayerIsBoostingChanged(PlayerModel playerModel, bool value)
    {
        if (controller != null)
            controller.isBoosting = value;
    }

    private void Start()
    {
        if (realtimeView.isOwnedLocallyInHierarchy)
        {
            if (statsEntity == null)
            {
                var _temp = Realtime.Instantiate("StatEntity",
                    position: transform.position,
                    rotation: Quaternion.identity,
                    ownedByClient: true,
                    preventOwnershipTakeover: true,
                    destroyWhenOwnerOrLastClientLeaves: true,
                    useInstance: realtime);
                statsEntity = _temp.GetComponent<StatsEntity>();
                GameManager.instance.RecordRIGO(_temp);
            }
            else
            {
                statsEntity.ResetStats();
            }

            StatsManager.instance.localStatsEntity = statsEntity;
        }
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
        model.meleePower = controller.meleeDamageModifier;
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
        //Debug.LogWarning("Calculated Damage for " + PlayerManager.instance.PlayerName(realtimeView.ownerIDInHierarchy) + " is " + calculatedDamage);
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