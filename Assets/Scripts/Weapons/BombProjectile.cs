using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : WeaponProjectileBase
{
    public Collider ColliderToArm;

    public float BombEjectionSpeed;

    public override void Fire(Transform _barrelTip, float _tipVelocity)
    {
        ColliderToArm.enabled = false;
        StartCoroutine(DelayActivation(1f));
        base.Fire(_barrelTip, mf_carVelocity);
        rb.AddForce(
            -transform.forward * (startSpeed + mf_carVelocity) * BombEjectionSpeed,
            ForceMode.VelocityChange);
    }

    private IEnumerator DelayActivation(float waitTime)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(waitTime);
        ColliderToArm.enabled = true;
    }
}