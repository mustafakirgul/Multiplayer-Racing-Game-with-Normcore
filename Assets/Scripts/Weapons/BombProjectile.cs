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
        if (!isNetworkInstance)
        {
            base.Fire(_barrelTip, mf_carVelocity);
            rb.AddForce(
                -transform.forward * (startSpeed + mf_carVelocity) * BombEjectionSpeed,
                ForceMode.VelocityChange);
        }
        StartCoroutine(DelayActivation(1f));
    }

    private IEnumerator DelayActivation(float waitTime)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(waitTime);
        ColliderToArm.enabled = true;
    }
}