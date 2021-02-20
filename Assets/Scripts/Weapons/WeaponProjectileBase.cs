using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Normal.Realtime;

public class WeaponProjectileBase : RealtimeComponent<ProjectileModel>
{
    public float startSpeed;
    public float damage;
    public float explosiveRange;
    public float weaponLifeTime;
    public float truckDamageFactor;
    public float truckDamageTempModifier;

    public float weaponFireRate;

    protected float mf_carVelocity;

    public GameObject projectile_Mesh;

    public Texture2D ProjectileToDisplay;

    //TODO: damage types?
    //TODO: weapon upgrades?

    protected Rigidbody rb;
    GameObject explosion;
    WaitForSeconds wait1Sec;
    Collider[] colliders;
    public bool isNetworkInstance = true;
    RealtimeView _realtimeView;
    RealtimeTransform _realtimeTransform;
    bool isExploded = false;
    List<GameObject> damagedPlayers;
    private Coroutine hitCoroutine, hitNoDamageCoroutine;

    UIManager UIManager;

    public int ProjectileID;

    protected override void OnRealtimeModelReplaced(ProjectileModel previousModel, ProjectileModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);

        if (previousModel != null)
        {
            previousModel.explodedDidChange -= UpdateExplosionState;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                currentModel.exploded = false;
            }

            UpdateModel();
            currentModel.explodedDidChange += UpdateExplosionState;
        }
    }

    private void UpdateExplosionState(ProjectileModel projectileModel, bool value)
    {
        if (explosion != null)
        {
            explosion.SetActive(true);
        }

        IsExplodedChanged();
    }

    private void IsExplodedChanged()
    {
        isExploded = model.exploded;
    }

    private void Awake()
    {
        colliders = new Collider[0];
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();
        rb = GetComponent<Rigidbody>();
        UIManager = FindObjectOfType<UIManager>();
    }

    void KillTimer()
    {
        //hitCoroutine = StartCoroutine(HitCR());
        GetComponent<TrailRenderer>().emitting = false;
        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        projectile_Mesh.SetActive(false);
        if (realtimeView.isOwnedLocallyInHierarchy) Realtime.Destroy(gameObject);
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
        _realtimeView.SetOwnership(PlayerManager.instance.localPlayerID);
        _realtimeTransform.SetOwnership(PlayerManager.instance.localPlayerID);

        if (_realtimeView.isOwnedLocallyInHierarchy)
        {
            Invoke(nameof(KillTimer), weaponLifeTime);
            isNetworkInstance = false;
        }
    }

    protected void UpdateModel()
    {
        isExploded = model.exploded;
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
        if (hitCoroutine != null)
        {
            StartCoroutine(HitCR());
        }
    }

    IEnumerator HitCR()
    {
        GetComponent<TrailRenderer>().emitting = false;
        GetComponent<Collider>().enabled = false;
        projectile_Mesh.SetActive(false);
        colliders = Physics.OverlapSphere(transform.position, explosiveRange);
        damagedPlayers = new List<GameObject>();
        if (colliders != null)
        {
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (!damagedPlayers.Contains(colliders[i].gameObject))
                    {
                        damagedPlayers.Add((colliders[i].gameObject));
                        Vector3 _origin = colliders[i].transform.position - transform.position;
                        //Debug.Log("In Explosion Range:" + colliders[i]);

                        if (colliders[i].gameObject.GetComponent<Player>() != null)
                        {
                            Player _player = colliders[i].gameObject.GetComponent<Player>();

                            if (ProjectileID != PlayerManager.instance.localPlayerID)
                            {
                                _player.ChangeExplosionForce(_origin);
                                _player.DamagePlayer(damage);
                                UIManager.ConfirmHitDamage();
                            }
                        }
                        else if (colliders[i].gameObject.GetComponent<Truck>() != null)
                        {
                            Truck _tempTruck = colliders[i].gameObject.GetComponent<Truck>();
                            _tempTruck.AddExplosionForce(_origin);
                            _tempTruck.DamagePlayer(damage * (truckDamageFactor + truckDamageTempModifier));
                            if (ProjectileID == PlayerManager.instance.localPlayerID)
                                UIManager.ConfirmHitDamage();
                        }
                        else if (colliders[i].gameObject.GetComponent<Rigidbody>() != null)
                        {
                            colliders[i].gameObject.GetComponent<Rigidbody>()
                                .AddExplosionForce(20000f, transform.position - _origin, 20f, 1000f);
                        }
                    }
                }
            }
        }

        rb.isKinematic = true;
        yield return wait1Sec;
        yield return wait1Sec;
        explosion.SetActive(false);
        yield return wait1Sec;
        if (realtimeView.isOwnedLocallyInHierarchy) Realtime.Destroy(gameObject);
        hitCoroutine = null;
    }

    IEnumerator HitNoDmg()
    {
        //Once projectile hits, if this object isn't a networked spawned
        //Which means that only if this is a local projectile owned by the player
        //Make them stop moving and commence damage caculations
        //Explosions and animations etc
        if (!isNetworkInstance)
        {
            GetComponent<TrailRenderer>().emitting = false;
            //GetComponent<MeshRenderer>().enabled = false;
            projectile_Mesh.SetActive(false);
            yield return wait1Sec;
            yield return wait1Sec;
            explosion.SetActive(false);
            yield return wait1Sec;
            Realtime.Destroy(gameObject);
        }

        hitNoDamageCoroutine = null;
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

        //Only look at the root of the transform object
        if (other.gameObject.GetComponent<NewCarController>() != null)
        {
            if (other.gameObject.GetComponent<NewCarController>().ownerID == PlayerManager.instance.localPlayerID)
            {
                Debug.Log("Self hit");
                return;
            }

            Debug.Log("HIT: " + other.GetComponent<NewCarController>().ownerID);
            model.exploded = true;
            Hit();
        }
        else
        {
            model.exploded = true;
            Hit();
        }
    }
}