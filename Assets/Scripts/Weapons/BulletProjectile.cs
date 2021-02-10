using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : WeaponProjectileBase
{
    public float bulletSpeed;

    protected override void Update()
    {
        base.Update();

        if (rb != null && !isNetworkInstance)
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
            rb.velocity = this.transform.forward * (bulletSpeed + mf_carVelocity);
        }
        else
        {

            rb.velocity = this.transform.forward * (bulletSpeed + mf_carVelocity);
        }
    }
}