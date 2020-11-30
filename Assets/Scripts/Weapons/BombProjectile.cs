using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : WeaponProjectileBase
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void Fire(Transform _barrelTip, float _tipVelocity)
    {
        base.Fire(_barrelTip, mf_carVelocity);

        rb.AddForce(
        transform.forward * (startSpeed + mf_carVelocity),
        ForceMode.VelocityChange);
    }
}
