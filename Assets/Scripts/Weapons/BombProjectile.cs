using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class BombProjectile : WeaponProjectileBase
{
    public Collider ColliderToArm;
    public float BombEjectionSpeed;

    public bool isArmed = false;

    protected override void Awake()
    {
        base.Awake();
        isArmed = false;
    }

    public override void Fire(Transform _barrelTip, float _tipVelocity)
    {
        if (_realtimeView.isOwnedLocallyInHierarchy)
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
        isArmed = true;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isArmed)
        {
            base.OnTriggerEnter(other);
        }
    }
}