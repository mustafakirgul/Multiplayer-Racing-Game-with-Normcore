using System;
using Normal.Realtime;
using UnityEngine;

public class TruckMelee : MonoBehaviour
{
    private Truck parent;

    private void Start()
    {
        parent = transform.parent.GetComponent<Truck>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var melee = other.transform.GetComponent<Melee>();
        if (melee != null)
        {
            var rtview = melee.transform.GetComponent<RealtimeView>();
            if (melee.controller.isBoosting)
            {
                parent.RegisterDamage(30f * melee.controller.meleeDamageModifier, rtview);
            }
            else if (rtview.isOwnedLocallyInHierarchy)
            {
                melee.player.DamagePlayer(50f);
            }
        }
    }
}