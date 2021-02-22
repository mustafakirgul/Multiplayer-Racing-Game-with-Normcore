using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : WeaponProjectileBase
{
    public float bulletSpeed;

    public void Update()
    {
        if (rb != null && _realtimeView.isOwnedLocallyInHierarchy)
        {
            BulletBrain();
        }
    }

    private void BulletBrain()
    {
        //Missile keeps going forward unless it finds a target
        if (mf_carVelocity < 10)
        {
            mf_carVelocity = 10;
            rb.velocity = transform.forward * (bulletSpeed + mf_carVelocity);
        }
        else
        {
            rb.velocity = transform.forward * (bulletSpeed + mf_carVelocity);
        }
    }
}