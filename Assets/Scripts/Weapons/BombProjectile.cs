using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : WeaponProjectileBase
{
    public Collider ColliderToArm;

    public float BombEjectionSpeed;

    protected override void Update()
    {
        base.Update();
    }

    public override void Fire(Transform _barrelTip, float _tipVelocity)
    {
        StartCoroutine(DelayActivation(1f));

        base.Fire(_barrelTip, mf_carVelocity);
        rb.AddForce(
            -transform.forward * (startSpeed + mf_carVelocity) * BombEjectionSpeed,
            ForceMode.VelocityChange);
    }

    private IEnumerator DelayActivation(float waitTime)
    {
        ColliderToArm.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(waitTime);
        ColliderToArm.enabled = true;
    }
}