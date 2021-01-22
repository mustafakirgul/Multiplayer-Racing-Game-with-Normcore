using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class WeaponProjectileBase : RealtimeComponent<ProjectileModel>
{
    public float startSpeed;
    public float damage;
    public float explosiveRange;
    public float weaponLifeTime;
    public float truckDamageFactor;

    protected float mf_carVelocity;

    public GameObject projectile_Mesh;

    //TODO: damage types?
    //TODO: weapon upgrades?

    protected Rigidbody rb;
    GameObject explosion;
    WaitForSeconds wait1Sec;
    Collider[] colliders;
    public bool isNetworkInstance = true;
    public int originOwnerID = -1;
    RealtimeView _realtimeView;
    RealtimeTransform _realtimeTransform;
    bool isExploded = false;

    protected override void OnRealtimeModelReplaced(ProjectileModel previousModel, ProjectileModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.explodedDidChange -= UpdateExplosionState;
        }

        if (currentModel != null)
        {
            currentModel.explodedDidChange += UpdateExplosionState;
        }
    }

    private void UpdateExplosionState(ProjectileModel model, bool _ifExploded)
    {
        //Checks for explosion of networked projectiles
        //Once state changes sync with server to make projectile explodes
        if (isExploded != model.exploded) // there is a change
        {
            if (model.exploded) // explosion
            {
                //No matter if local or server instance, when the projectile explosion state
                //is updated and if there is an explosion animation, activate it
                if (explosion != null)
                {
                    explosion.SetActive(true);
                }
                isExploded = model.exploded;
                StartCoroutine(HitCR());
            }
        }
    }

    private void Awake()
    {
        colliders = new Collider[0];
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();
        rb = GetComponent<Rigidbody>();
    }

    void KillTimer()
    {
        Hit();
    }

    protected virtual void Start()
    {
        wait1Sec = new WaitForSeconds(1f);

        //Set cosmetic explosion to false
        explosion = transform.GetChild(0).gameObject;
        explosion.SetActive(false);
        model.exploded = false;

        //Check to owner of the projectile
        //Obtain reference to scripts
        originOwnerID = _realtimeTransform.ownerIDSelf;
        _realtimeView.SetOwnership(originOwnerID);
        _realtimeTransform.SetOwnership(originOwnerID);
        if (_realtimeView.isOwnedLocallySelf)
        {
            Invoke(nameof(KillTimer), weaponLifeTime);
            isNetworkInstance = false;
        }
        else
        {
            GetComponent<Collider>().enabled = false;
        }
    }

    protected virtual void Update()
    {
        if (!isNetworkInstance)
        {
            _realtimeView.RequestOwnership();
            _realtimeTransform.RequestOwnership();
        }
    }

    public virtual void Fire(Transform _barrelTip, float _tipVelocity)
    {
        //Save float to pass down to children for bomb or forward projectile related weapons
        mf_carVelocity = _tipVelocity;

        rb = GetComponent<Rigidbody>();
        wait1Sec = new WaitForSeconds(1f);

        //Only apply kinematics after missile explosion occurs
        //Let local physics be done on local machine
        if (isNetworkInstance)
        {
            rb.isKinematic = true;
            return;
        }

        //GetComponent<MeshRenderer>().enabled = true;
        projectile_Mesh.SetActive(true);
        transform.position = _barrelTip.position;
        transform.rotation = _barrelTip.rotation;
    }

    void Hit()
    {
            StartCoroutine(HitCR());
    }

    void HitBlank()
    {
        if (!isNetworkInstance)
        {
            StartCoroutine(HitNoDmg());
        }
        else
        {
            model.exploded = true;
        }
    }

    IEnumerator HitCR()
    {
        GetComponent<TrailRenderer>().emitting = false;
        colliders = Physics.OverlapSphere(transform.position, explosiveRange);

        if (colliders != null)
        {
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    Vector3 _origin = colliders[i].transform.position - transform.position;
                    //Debug.Log("In Explosion Range:" + colliders[i]);

                    if (colliders[i].gameObject.GetComponent<Player>() != null)
                    {
                        Player _player = colliders[i].gameObject.GetComponent<Player>();
                        _player.ChangeExplosionForce(_origin);
                        _player.DamagePlayer(damage);
                    }
                    else if (colliders[i].gameObject.GetComponent<Truck>() != null)
                    {
                        Truck _tempTruck = colliders[i].gameObject.GetComponent<Truck>();
                        _tempTruck.AddExplosionForce(_origin);
                        _tempTruck.DamagePlayer(damage * truckDamageFactor);
                    }
                    else if (colliders[i].gameObject.GetComponent<Rigidbody>() != null)
                    {
                        colliders[i].gameObject.GetComponent<Rigidbody>()
                            .AddExplosionForce(20000f, transform.position - _origin, 20f, 1000f);
                    }
                }
            }
        }

        projectile_Mesh.SetActive(false);
        yield return wait1Sec;
        yield return wait1Sec;
        explosion.SetActive(false);
        yield return wait1Sec;
        Realtime.Destroy(gameObject);
    }

    IEnumerator HitNoDmg()
    {
        //Once projectile hits, if this object isn't a networked spawned
        //Which means that only if this is a local projectile owned by the player
        //Make them stop moving and commence damage caculations
        //Explosions and animations etc
        if (!isNetworkInstance)
        {
            rb.isKinematic = true;
        }

        GetComponent<TrailRenderer>().emitting = false;
        colliders = Physics.OverlapSphere(transform.position, explosiveRange);

        if (colliders != null)
        {
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    Vector3 _origin = colliders[i].transform.position - transform.position;
                    //Debug.Log("In Explosion Range:" + colliders[i]);
                    if (colliders[i].gameObject.GetComponent<Player>() != null)
                    {
                        colliders[i].gameObject.GetComponent<Player>().ChangeExplosionForce(_origin);
                        colliders[i].gameObject.GetComponent<Player>().DamagePlayer(0);
                    }
                    else if (colliders[i].gameObject.GetComponent<Truck>() != null)
                    {
                        colliders[i].gameObject.GetComponent<Truck>().AddExplosionForce(_origin);
                    }

                    else if (colliders[i].gameObject.GetComponent<Rigidbody>() != null)
                    {
                        colliders[i].gameObject.GetComponent<Rigidbody>()
                            .AddExplosionForce(20000f, transform.position - _origin, 20f, 1000f);
                    }
                }
            }
        }

        //GetComponent<MeshRenderer>().enabled = false;
        projectile_Mesh.SetActive(false);
        yield return wait1Sec;
        yield return wait1Sec;
        explosion.SetActive(false);
        yield return wait1Sec;
        Realtime.Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //TODO Logic for target type detection
        //If this is a networked projectile, let the local physic caculation
        //take place and report on the explosion state of the collision
        //Must immediately apply physics the moment projectile hits something
        if (isNetworkInstance)
        {
            return;
        }
        else if (rb != null)
        {
            rb.isKinematic = true;
        }

        //Only look at the root of the transform object
        if (other.gameObject.GetComponent<NewCarController>() != null)
        {
            if (other.gameObject.GetComponent<NewCarController>().ownerID == originOwnerID)
            {
                Debug.Log("Self hit");
                return;
            }
            else
            {
                Debug.Log("HIT: " + other.GetComponent<NewCarController>().ownerID);
                model.exploded = true;
                Hit();
            }
        }
        else
        {
            model.exploded = true;
            Hit();
        }
    }
}