using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : WeaponProjectileBase
{
    [SerializeField]
    private float bulletSpeed;
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
        rb.velocity = this.transform.forward * (bulletSpeed + mf_carVelocity);
    }
}
