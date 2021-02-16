using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileProjectile : WeaponProjectileBase
{
    public float missileSpeed;
    public float missileRotationSpeed;
    public float missileDectectionRange;
    public float missileRadarRefresh;

    public LayerMask MissileDetectionLayer;

    public Collider[] MissileTargets;

    [SerializeField] private Transform LockedTarget;
    // Start is called before the first frame update

    protected override void Start()
    {
        base.Start();
        if (!isNetworkInstance)
        {
            MissileTargets = new Collider[0];
            StartCoroutine(DetectTarget());
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (rb != null && !isNetworkInstance)
        {
            MissileBrain();
            AdjustMissileCourse();
        }
    }

    private void MissileBrain()
    {
        //Missile keeps going forward unless it finds a target
        rb.velocity = this.transform.forward * (missileSpeed + mf_carVelocity);
    }

    private IEnumerator DetectTarget()
    {
        while (true)
        {
            MissileDetection();
            yield return new WaitForSeconds(missileRadarRefresh);
        }
    }

    private void AdjustMissileCourse()
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
        Gizmos.DrawSphere(transform.position, missileDectectionRange);
    }

    private void MissileDetection()
    {
        MissileTargets = Physics.OverlapSphere(transform.position, missileDectectionRange, MissileDetectionLayer);

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
                    if (MissileTargets[i].transform.root.GetComponent<NewCarController>().ownerID
                        != PlayerManager.localPlayerID)
                    {
                        LockedTarget = MissileTargets[i].transform;
                        return;
                    }
                }
            }
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!isNetworkInstance)
        {
            base.OnTriggerEnter(other);
        }
    }
}