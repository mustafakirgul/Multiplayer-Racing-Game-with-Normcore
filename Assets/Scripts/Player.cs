﻿using System;
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
    public float tempDefenseModifier = 0f;
    public float healModifier = 0f;
    private NewCarController controller;

    protected override void OnRealtimeModelReplaced(PlayerModel previousModel, PlayerModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.playerNameDidChange -= PlayerNameChanged;
            previousModel.healthDidChange -= PlayerHealthChanged;
            previousModel.forcesDidChange -= PlayerForcesChanged;
            previousModel.idDidChange -= IDChanged;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                currentModel.playerName = GameManager.instance.playerName;
                currentModel.health = maxPlayerHealth;
                currentModel.id = realtimeView.ownerIDInHierarchy;
                controller.ownerID = realtimeView.ownerIDInHierarchy;
                PlayerManager.instance.localPlayerID = realtimeView.ownerIDInHierarchy;
                Debug.LogWarning("PlayerID set to: " + realtimeView.ownerIDInHierarchy);
                
            }

            currentModel.playerNameDidChange += PlayerNameChanged;
            currentModel.healthDidChange += PlayerHealthChanged;
            currentModel.forcesDidChange += PlayerForcesChanged;
            currentModel.idDidChange += IDChanged;
        }
    }

    private void Awake()
    {
        controller = GetComponent<NewCarController>();
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
        model.health -= ((1 - armourDefenseModifier - tempDefenseModifier * 0.5f) * damage);

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

    private void IDChanged(PlayerModel playerModel, int value)
    {
        _id = value;
        controller.ownerID = _id;
        if (realtimeView.isOwnedLocallyInHierarchy)
        {
            PlayerManager.instance.localPlayerID = _id;
            Debug.LogWarning("PlayerID set to: " + _id);
        }
    }

    private void PlayerForcesChanged(PlayerModel playerModel, Vector3 value)
    {
        explosionForce = model.forces;
    }

    private void PlayerNameChanged(PlayerModel playerModel, string value)
    {
        playerName = model.playerName;
        controller.IDDisplay.SetText(playerName);
        controller._currentName = model.playerName;
    }
}