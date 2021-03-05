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

    public float barrelFireAngle;

    protected float mf_carVelocity;

    public GameObject projectile_Mesh;

    public Texture2D ProjectileToDisplay;

    //TODO: damage types?
    //TODO: weapon upgrades?

    protected Rigidbody rb;
    GameObject explosion;
    WaitForSeconds wait1Sec;
    Collider[] colliders;
    public RealtimeView _realtimeView;
    bool isExploded;
    List<GameObject> damagedPlayers;

    private Coroutine hitCoroutine = null;
    private Coroutine cr_CosmeticExplode = null;

    UIManager UIManager;

    public StatsEntity statEntity;

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
                isExploded = currentModel.exploded;
            }

            currentModel.explodedDidChange += UpdateExplosionState;
        }
    }

    private void UpdateExplosionState(ProjectileModel projectileModel, bool value)
    {
        IsExplodedChanged();
    }

    private void IsExplodedChanged()
    {
        isExploded = model.exploded;
    }

    protected virtual void Awake()
    {
        colliders = new Collider[0];
        _realtimeView = realtimeView;
        rb = GetComponent<Rigidbody>();
        UIManager = FindObjectOfType<UIManager>();
    }

    void KillTimer()
    {
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

        if (realtimeView.isOwnedLocallyInHierarchy)
        {
            Invoke(nameof(KillTimer), weaponLifeTime);
        }
    }

    protected void Update()
    {
        if (realtimeView.isOwnedRemotelyInHierarchy
        ) //if this projectile is a network instance it will check the model to see if the owner had changed it
        {
            isExploded = model.exploded;
            if (isExploded) //if main model exploded, explode the instance, cosmetically
            {
                CosmeticExplode();
            }
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
        if (_realtimeView.isOwnedRemotelyInHierarchy)
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
        if (hitCoroutine != null) return;
        hitCoroutine = StartCoroutine(HitCR());
    }

    void Hit(Truck truck)
    {
        truck.RegisterDamage(damage, realtimeView);
    }

    void Hit(NewCarController car)
    {
        car.RegisterDamage(damage, realtimeView);
    }

    IEnumerator HitCR()
    {
        rb.isKinematic = true;
        colliders = Physics.OverlapSphere(transform.position, explosiveRange);
        damagedPlayers = new List<GameObject>();
        GameObject _tempCollisionObject;
        if (colliders != null)
        {
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    _tempCollisionObject = colliders[i].gameObject;
                    if (!damagedPlayers.Contains(_tempCollisionObject))
                    {
                        damagedPlayers.Add(_tempCollisionObject);
                        if (_tempCollisionObject.GetComponent<Rigidbody>() != null)
                        {
                            if (_tempCollisionObject.GetComponent<RealtimeView>() != null &&
                                _tempCollisionObject.GetComponent<LootContainer>() != null)
                            {
                                _tempCollisionObject.GetComponent<RealtimeView>().RequestOwnership();
                            }

                            _tempCollisionObject.GetComponent<Rigidbody>()
                                .AddExplosionForce(20000f,
                                    transform.position,
                                    20f,
                                    1000f);
                        }
                    }
                }
            }
        }

        hitCoroutine = null;
        yield return null;
    }

    public void CosmeticExplode()
    {
        if (cr_CosmeticExplode != null) StopCoroutine(cr_CosmeticExplode);
        cr_CosmeticExplode = StartCoroutine(CR_CosmeticExplode());
    }

    IEnumerator CR_CosmeticExplode()
    {
        GetComponent<TrailRenderer>().emitting = false;
        projectile_Mesh.SetActive(false);
        explosion.SetActive(true);
        if (realtimeView.isOwnedLocallyInHierarchy) model.exploded = true;
        yield return wait1Sec;
        yield return wait1Sec;
        explosion.SetActive(false);
        yield return wait1Sec;
        cr_CosmeticExplode = null;
        if (realtimeView.isOwnedLocallyInHierarchy) Realtime.Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //if no realtime component in collided object then just detonate the projectile/explosive 
        RealtimeView _tempRTView = other.gameObject.GetComponent<RealtimeView>();
        //if the collided object has a realtimeview
        if (_tempRTView != null)
        {
            //now we need to check if the collided object is the truck, because if it is the truck, we need to damage it even if we own the projectile
            Truck _tempTruck = other.gameObject.GetComponent<Truck>();
            if (_tempTruck != null) //if it is a truck
            {
                if (realtimeView.isOwnedLocallyInHierarchy && statEntity != null) //record stat if you are owned locally
                    statEntity.ReceiveStat(StatType.damage, damage);


                //damage the truck, but only if it is owned locally (you know why)
                if (_tempTruck.realtimeView.isOwnedLocallyInHierarchy)
                {
                    //Debug.LogWarning("Truck hit!");
                    UIManager.ConfirmHitDamage();
                    Hit(_tempTruck);
                    CosmeticExplode();
                    return;
                }
            }

            //if self hit return
            if (_tempRTView.realtimeView.ownerIDInHierarchy == realtimeView.ownerIDInHierarchy) return;

            //check if it is a car
            NewCarController _tempCar = other.gameObject.GetComponent<NewCarController>();
            if (_tempCar != null) //if it is a car, go for it!
            {
                if (_tempCar._realtimeView.isOwnedLocallyInHierarchy)
                {
                    //Debug.LogWarning("Car hit!");
                    UIManager.ConfirmHitDamage();
                    Hit(_tempCar);
                    CosmeticExplode();
                    return;
                }
            }

            //if it is not a truck, first check this:
            if (realtimeView.isOwnedLocallyInHierarchy)
                return; //do nothing if the projectile/explosive is owned by the localPlayer (which means all other player cars in your scene are network instances)
            if (_tempRTView.isOwnedRemotelyInHierarchy)
                return; // do nothing if the collided object is not owned by the local player because it would not make any difference on the main model if a network instance tries to register damage
        }

        //Debug.LogWarning("Empty hit!");
        CosmeticExplode();
        Hit();
    }

    public void RegisterKill()
    {
        if (realtimeView.isOwnedLocallyInHierarchy) //record stat if you are owned locally
        {
            //Debug.LogWarning("1 kill registered for " + PlayerManager.instance.PlayerName(realtimeView.ownerIDInHierarchy));
            statEntity.ReceiveStat(StatType.kill);
        }
    }
}