using System.Collections;
using System.Collections.Generic;
using Normal.Realtime;
using UnityEngine;

public class MissileProjectile : WeaponProjectileBase
{
    public float missileSpeed;
    public float missileRotationSpeed;
    public float missileDectectionRange;
    public float missileRadarRefresh;

    LayerMask LayersToTarget;

    public Collider[] MissileTargets;

    [SerializeField] private Transform LockedTarget;
    // Start is called before the first frame update

    protected override void Start()
    {
        base.Start();
        if (_realtimeView.isOwnedLocallyInHierarchy)
        {
            MissileTargets = new Collider[0];
            StartCoroutine(DetectTarget());

            LayersToTarget = (1 << LayerMask.NameToLayer("WeaponTargets") | 1 << LayerMask.NameToLayer("Truck"));
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (rb != null && _realtimeView.isOwnedLocallyInHierarchy)
        {
            MissileBrain();
            AdjustMissileCourse(LockedTarget);
        }
    }

    public void SetTarget(Transform Target)
    {
        LockedTarget = Target;
    }

    private void MissileBrain()
    {
        //Missile keeps going forward unless it finds a target
        rb.velocity = this.transform.forward * (missileSpeed + mf_carVelocity);
    }

    private IEnumerator DetectTarget()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            if(LockedTarget == null)
            {
                MissileDetection();
                yield return new WaitForSeconds(missileRadarRefresh);
            }
            yield return null;
        }
    }

    private void AdjustMissileCourse(Transform LockedTarget)
    {
        if (LockedTarget != null)
        {
            //Determine possibly random target or target that is directly in front of the current car
            //Don't do this, just get the target in front
            var targetRotation = Quaternion.LookRotation(LockedTarget.position - transform.localPosition);

            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation,
                missileRotationSpeed * Time.deltaTime));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, missileDectectionRange);
    }

    private void MissileDetection()
    {
        MissileTargets = Physics.OverlapSphere(transform.position, missileDectectionRange, LayersToTarget);

        if (MissileTargets.Length == 0)
        {
            return;
        }
        else
        {
            //Detection logic for homing missile
            for (int i = 0; i < MissileTargets.Length; i++)
            {
                //To Do: add in a new system for target selection/priority
                //Logic to prioritize targets
                if (MissileTargets[i].transform.root.tag == "target")
                {
                    LockedTarget = MissileTargets[i].transform;
                    return;
                }

                if (MissileTargets[i].transform.root.GetComponent<NewCarController>())
                {
                    if (MissileTargets[i].transform.root.GetComponent<RealtimeView>().ownerIDInHierarchy
                        != _realtimeView.ownerIDInHierarchy)
                    {
                        LockedTarget = MissileTargets[i].transform;
                        return;
                    }
                }
            }
        }
    }
}