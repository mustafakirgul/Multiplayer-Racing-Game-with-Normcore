using System;
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
                currentModel.exploded = false;
            }

            currentModel.explodedDidChange += UpdateExplosionState;
        }

        isExploded = model.exploded;
    }

    private void UpdateExplosionState(ProjectileModel projectileModel, bool value)
    {
        IsExplodedChanged();
    }

    private void IsExplodedChanged()
    {
        isExploded = model.exploded;
    }

    protected void Update()
    {
        if (realtimeView.isOwnedRemotelyInHierarchy
        ) //if this projectile is a network instance it will check the model to see if the owner had changed it
        {
            if (model.exploded) //if main model exploded, explode the instance, cosmetically
            {
                CosmeticExplode();
            }
        }
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
            if (GetComponent<BombProjectile>() == null)
            {
                Invoke(nameof(KillTimer), weaponLifeTime);
            }
            else
            {
            }
        }
    }

    public virtual void Fire(Transform _barrelTip, float _tipVelocity)
    {
        //Save float to pass down to children for bomb or forward projectile related weapons
        mf_carVelocity = _tipVelocity;

        rb = GetComponent<Rigidbody>();
        wait1Sec = new WaitForSeconds(1f);
        projectile_Mesh.SetActive(true);
        transform.position = _barrelTip.position;
        transform.rotation = _barrelTip.rotation;
    }

    void Hit(Truck truck)
    {
        truck.RegisterDamage(damage, realtimeView);
    }

    void Hit(Player player)
    {
        player.gameObject.GetComponent<NewCarController>().RegisterDamage(damage, realtimeView);
    }

    public void CosmeticExplode()
    {
        if (realtimeView.isOwnedLocallyInHierarchy) model.exploded = true;
        if (cr_CosmeticExplode == null) cr_CosmeticExplode = StartCoroutine(CR_CosmeticExplode());
    }

    IEnumerator CR_CosmeticExplode()
    {
        GetComponent<TrailRenderer>().emitting = false;
        projectile_Mesh.SetActive(false);
        rb.isKinematic = true;
        colliders = Physics.OverlapSphere(transform.position, explosiveRange);
        damagedPlayers = new List<GameObject>();
        if (colliders != null)
        {
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    var _tempCollisionObject = colliders[i].gameObject;
                    if (!damagedPlayers.Contains(_tempCollisionObject))
                    {
                        damagedPlayers.Add(_tempCollisionObject);
                        Truck truck = _tempCollisionObject.GetComponent<Truck>();
                        if (truck != null)
                        {
                            if (realtimeView.isOwnedLocallyInHierarchy)
                            {
                                UIManager.ConfirmHitDamage();
                                if (statEntity != null && !truck.isInvincible && truck._health > 0)
                                    statEntity.ReceiveStat(StatType.damage, damage);
                            }

                            if (truck.realtimeView.isOwnedLocallyInHierarchy)
                                Hit(truck);
                        }
                        else
                        {
                            Player player = _tempCollisionObject.GetComponent<Player>();
                            if (player != null)
                            {
                                if (realtimeView.isOwnedLocallyInHierarchy)
                                {
                                    UIManager.ConfirmHitDamage();
                                }

                                Hit(player);
                            }
                            else
                            {
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
            }
        }

        rb.isKinematic = true;
        explosion.SetActive(true);
        yield return wait1Sec;
        explosion.SetActive(false);
        yield return wait1Sec;
        if (realtimeView.isOwnedLocallyInHierarchy) Realtime.Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Truck>() != null)
        {
            CosmeticExplode();
        }
        else
        {
            RealtimeView rt = other.gameObject.GetComponent<RealtimeView>();
            if (rt != null)
            {
                if (GetComponent<BombProjectile>() != null)
                {
                    CosmeticExplode();
                }
                else if (rt.ownerIDInHierarchy != realtimeView.ownerIDInHierarchy)
                {
                    CosmeticExplode();
                }
            }
        }
    }
}