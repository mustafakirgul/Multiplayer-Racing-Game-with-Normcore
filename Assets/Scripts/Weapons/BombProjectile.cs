using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : WeaponProjectileBase
{
    // Update is called once per frame
    [SerializeField]
    private Collider ColliderToArm;
    protected override void Update()
    {
        base.Update();
    }

    public override void Fire(Transform _barrelTip, float _tipVelocity)
    {
        StartCoroutine(DelayActivation(0.1f));

        base.Fire(_barrelTip, mf_carVelocity);
        rb.AddForce(
        -transform.forward * (startSpeed + mf_carVelocity),
        ForceMode.VelocityChange);
    }

    private IEnumerator DelayActivation(float waitTime)
    {
        ColliderToArm.enabled = false;
        yield return new WaitForSeconds(waitTime);
        ColliderToArm.enabled = true;
    }
}
