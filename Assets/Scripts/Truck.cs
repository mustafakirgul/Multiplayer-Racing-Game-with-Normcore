using System;
using UnityEngine;
using Normal.Realtime;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class Truck : RealtimeComponent<TruckModel>
{
    public TruckWheel[] _wheels;
    public float maxTorque; //per powered wheel

    [Range(0f, 1f)] public float _torqueFactor;
    [Range(-20f, 20f)] public float _steeringAngle;
    public float maxSteeringAngle;
    public bool _handBrake;
    int _length;
    Vector3 _position;
    Quaternion _rotation;
    RealtimeTransform _rTTransform;
    List<RealtimeTransform> _rTTransforms;
    public Vector3 explosionPoint;
    Transform _ownerTransform;
    Rigidbody truckBody;
    Vector3 _startPosition => new Vector3(0, 30, 0);
    [Space] [Header("Loot Settings")] public Vector3 lootLaunchPoint;
    [Range(0, 1)] public float lootChance;
    public float throwForce;

    [Space]
    //Truck Way Points System
    [SerializeField]
    private List<WayPoint> m_wayPoints;

    [SerializeField] private Transform currentWPT;
    [SerializeField] private float steerRefreshTimer = 1f / 60f;
    [SerializeField] private int currentWPindex = 0;
    public float waypointSwitchThreshold;
    private bool damageFeedback;

    WaitForSeconds waitASecond;

    public GameObject damageSphere;

    private Vector3 _tempSizeForDamageSphere;

    private Coroutine cr_DamageFeedback;
    public float damageDisplayTime;
    private WaitForSeconds wait;

    public RealtimeTransform rtTransform => GetComponent<RealtimeTransform>();
    public bool isInvincible = true;

    //X-Ray silhouette
    public Renderer TruckMesh;
    public Outline TruckOutline;

    private bool postBoom = false;

    //Torque adjustment relative to elevation trend of the ground
    [SerializeField]
    private float angle, elevationTorqueFactor, elevationConstant, currentTorquePerWheel, torqueBoostAngleLimit;

    private int theKiller;

    public bool isBoombastic;
    public float boombasticModeY = 25f;
    public float boombasticModeDuration = 33f;
    public ParticleSystem boombasticShield;
    private SphereCollider shieldCollider => boombasticShield.transform.GetComponent<SphereCollider>();
    private Rigidbody rb => GetComponent<Rigidbody>();

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + lootLaunchPoint, 1f);
    }

    public void UpdateTorqueFactor(float _f)
    {
        _torqueFactor = _f;
        //Debug.LogWarning("Truck Torque Factor = " + _torqueFactor);
    }

    public void Handrake(bool state)
    {
        _handBrake = state;
    }

    private void Awake()
    {
        truckBody = GetComponent<Rigidbody>();
        _rTTransform = GetComponent<RealtimeTransform>();
        _rTTransforms = new List<RealtimeTransform>();
        _rTTransforms.AddRange(GetComponentsInChildren<RealtimeTransform>());
        _rTTransforms.Add(GetComponent<RealtimeTransform>());
        _length = _wheels.Length;
        var centerOfMass = truckBody.centerOfMass;
        centerOfMass = new Vector3(centerOfMass.x, centerOfMass.y - 5, centerOfMass.z);
        truckBody.centerOfMass = centerOfMass;
        wait = new WaitForSeconds(damageDisplayTime);
    }

    private void Start()
    {
        if (realtimeView.isOwnedRemotelyInHierarchy)
        {
            damageSphere.SetActive(false);
            return;
        }

        StartHealth();
        InitializWaypointAI();
        waitASecond = new WaitForSeconds(steerRefreshTimer);
        if (damageSphere == null)
            Debug.LogError("No damage sphere for truck!");
        else
        {
            damageFeedback = true;
            damageSphere.SetActive(false);
        }

        for (int i = 0; i < _wheels.Length; i++)
        {
            _wheels[i].model.GetComponent<RealtimeView>().RequestOwnership();
            _wheels[i].model.GetComponent<RealtimeView>().RequestOwnership();
            _wheels[i].model.GetComponent<RealtimeTransform>().RequestOwnership();
        }
    }

    public void SetInvincibility(bool state)
    {
        isInvincible = state;
    }

    public void InitializWaypointAI()
    {
        m_wayPoints = new List<WayPoint>();
        foreach (WayPoint waypoint in FindObjectsOfType<WayPoint>())
        {
            if (waypoint.gameObject.activeInHierarchy && !m_wayPoints.Contains(waypoint))
            {
                m_wayPoints.Add(waypoint);
            }
        }

        m_wayPoints = m_wayPoints.OrderBy(waypoint => waypoint.index).ToList();
        SetWayPoint(0); // start moving towards first waypoint
        StartCoroutine(WaypointAI()); // start 
    }

    private IEnumerator WaypointAI()
    {
        while (true)
        {
            while (currentWPT != null && realtimeView.isOwnedLocallyInHierarchy)
            {
#if UNITY_EDITOR
                waitASecond = new WaitForSeconds(steerRefreshTimer);
                //Debug.LogWarning("Server time: " + _realtime.room.time);
#endif
                float _distanceToTarget = Vector3.Distance(transform.position, currentWPT.position);
                //Debug.Log("MGNTD: " + _distanceToTarget);
                if (_distanceToTarget < waypointSwitchThreshold)
                {
                    currentWPindex++;
                    SetWayPoint(currentWPindex);
                }

                CalculateRoute();
                CalculateTorque();
                yield return waitASecond;
            }

            yield return waitASecond;
        }
    }

    private void CalculateTorque()
    {
        angle = -Mathf.Clamp(Vector3.Angle(Vector3.up, transform.forward) - elevationConstant, -torqueBoostAngleLimit,
            torqueBoostAngleLimit);
        elevationTorqueFactor = 1f + Mathf.Clamp01(angle / torqueBoostAngleLimit);
    }

    void SetWayPoint(int wayPointIndex)
    {
        if (m_wayPoints.Count == 0)
            return;
        currentWPT = m_wayPoints[wayPointIndex % m_wayPoints.Count].transform;
    }

    void CalculateRoute()
    {
        for (int i = 0; i < _length; i++)
        {
            _steeringAngle = maxSteeringAngle *
                             Vector3.Dot(
                                 Vector3.Cross(transform.forward,
                                     (currentWPT.position - transform.position).normalized), Vector3.up);

            if (_wheels[i].isSteeringWheel)
            {
                _wheels[i].collider.steerAngle =
                    Mathf.Lerp(_wheels[i].collider.steerAngle, _steeringAngle, Time.deltaTime * 20f);
            }

            if (_wheels[i].isReverseSteeringWheel)
            {
                _wheels[i].collider.steerAngle =
                    Mathf.Lerp(_wheels[i].collider.steerAngle, -_steeringAngle, Time.deltaTime * 20f);
            }
        }
    }

    void ResetTransform()
    {
        transform.position = _startPosition;
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (realtimeView.isOwnedRemotelyInHierarchy) return;
        if (isBoombastic)
        {
            BoombasticMode();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ResetTransform();
            }

            if (Input.GetKeyDown(KeyCode.K)) // Kill Ironhog
            {
                model.health = 0f;
            }

            if (Input.GetKeyDown(KeyCode.L)) // Drop Loot
            {
                DropLoot();
            }

            if (transform.position.y < -300)
            {
                ResetTransform();
            }

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                Vector3 pos = PlayerManager.instance.localPlayer.position;
                transform.position = new Vector3(pos.x, pos.y + 10f, pos.z);
            }

            currentTorquePerWheel = maxTorque * _torqueFactor * elevationTorqueFactor;
            if (_length > 0)
            {
                for (int i = 0; i < _length; i++)
                {
                    if (_handBrake)
                    {
                        _wheels[i].collider.motorTorque = 0f;
                        _wheels[i].collider.brakeTorque = maxTorque;
                    }
                    else if (_wheels[i].isPowered)
                    {
                        _wheels[i].collider.brakeTorque = 0f;
                        _wheels[i].collider.motorTorque = currentTorquePerWheel;
                    }


                    _wheels[i].collider.GetWorldPose(out _position, out _rotation);

                    _wheels[i].model.position = _position;

                    _wheels[i].model.rotation = _rotation;
                }
            }
        }
    }

    private void BoombasticMode()
    {
        if (Math.Abs(transform.position.y - boombasticModeY) > 1f)
        {
            rb.MovePosition(Vector3.Lerp(rb.position,
                new Vector3(rb.position.x, boombasticModeY, rb.position.z), Time.deltaTime * 6.66f));
            transform.up = Vector3.Lerp(transform.up, Vector3.up, Time.deltaTime * 6.66f);
        }
        else
        {
            transform.Rotate(Vector3.up, Time.deltaTime * 133.2f);
        }
    }

    public void AddExplosionForce(Vector3 _origin)
    {
        if (realtimeView.isOwnedLocallyInHierarchy)
        {
            truckBody.AddExplosionForce(200000f, transform.position - _origin, 20f, 1000f);
            ResetExplosionPoint();
        }
        else
        {
            if (_explosionForce != _origin)
            {
                ChangeExplosionForce(_origin);
            }
        }
    }

    #region MODEL INTERACTIONS

    public float _health;
    public float _maxHealth;
    public Vector3 _explosionForce;

    protected override void OnRealtimeModelReplaced(TruckModel previousModel, TruckModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.healthDidChange -= HealthChanged;
            previousModel.explosionPointDidChange -= ForcesChanged;
            previousModel.isBoombasticDidChange -= IsBoombasticChanged;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                StartHealth();
                currentModel.isBoombastic = false;
            }

            currentModel.healthDidChange += HealthChanged;
            currentModel.explosionPointDidChange += ForcesChanged;
            currentModel.isBoombasticDidChange += IsBoombasticChanged;
        }
    }

    private void ChangeIsBoombastic(bool value)
    {
        isBoombastic = value;
        model.isBoombastic = value;
        isInvincible = value;
        rb.isKinematic = value;
        rb.useGravity = !value;
        shieldCollider.enabled = value;
        Handrake(true);
        UpdateTorqueFactor(0f);
        if (value)
        {
            boombasticModeY += transform.position.y;
            StartCoroutine(CR_BackToNormal());
            boombasticShield.Play();
        }
        else
        {
            boombasticShield.Stop();
        }
    }

    private IEnumerator CR_BackToNormal()
    {
        yield return new WaitForSeconds(boombasticModeDuration);
        ChangeIsBoombastic(false);
        yield return null;
    }

    private void IsBoombasticChanged(TruckModel truckModel, bool value)
    {
        isBoombastic = value;
    }

    public void RegisterDamage(float damage, RealtimeView _realtimeView)
    {
        //Debug.LogWarning(PlayerManager.instance.PlayerName(_realtimeView.ownerIDInHierarchy) + " hit truck! | Damage: " + damage);
        theKiller = _realtimeView.ownerIDInHierarchy;
        DamagePlayer(damage);
    }

    public void ResetExplosionPoint()
    {
        model.explosionPoint = Vector3.zero;
    }

    public void ChangeExplosionForce(Vector3 _origin)
    {
        model.explosionPoint += _origin;
    }

    public void StartHealth()
    {
        if (GameManager.instance.isDebugBuild)
        {
            model.health = GameManager.instance.debugTruckHealth;
            currentWPindex = 0;
        }
        else
        {
            StartCoroutine(SetTruckScaleableHealthCR());
            currentWPindex = 0;
        }
    }

    private IEnumerator SetTruckScaleableHealthCR()
    {
        yield return new WaitForSeconds(5f);
        int numberOfPlayers = PlayerManager.instance.allPlayers.Length;
        _maxHealth = (350f * numberOfPlayers);
        model.health = (350f * numberOfPlayers);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void DamagePlayer(float damage)
    {
        if (!isInvincible)
        {
            model.health -= damage;
            DamageFeedback();
            DropRandomLoot();
        }

        if (_health < (_maxHealth / 2f) && !postBoom)
        {
            PlayerManager.instance.SpawnItems();
            postBoom = true;
            ChangeIsBoombastic(true);
        }
    }

    private void DamageFeedback()
    {
        if (!damageFeedback) return;
        if (cr_DamageFeedback != null)
            StopCoroutine(cr_DamageFeedback);
        cr_DamageFeedback = StartCoroutine(CR_DamageFeedback());
    }

    IEnumerator CR_DamageFeedback()
    {
#if UNITY_EDITOR
        wait = new WaitForSeconds(damageDisplayTime);
#endif
        yield return wait;
        damageSphere.SetActive(true);
        yield return wait;
        damageSphere.SetActive(false);
        cr_DamageFeedback = null;
    }

    private void DropRandomLoot()
    {
        if (UnityEngine.Random.Range(0f, 1f) < lootChance) //random chance of loot drop
        {
            DropLoot();
        }
    }

    private void DropLoot()
    {
        GameObject _temp = Realtime.Instantiate("Loot",
            position: transform.position + lootLaunchPoint,
            rotation: Quaternion.identity,
            ownedByClient:
            true,
            preventOwnershipTakeover:
            false,
            useInstance:
            realtime);
        int PUCount =
            (LootManager.instance.playerLootPoolSave.PlayerPowerUps.Count - 1);
        _temp.GetComponent<LootContainer>().SetID(UnityEngine.Random.Range(-PUCount, 666));
        Vector3 _tempDir = UnityEngine.Random.onUnitSphere;
        _tempDir = new Vector3(_tempDir.x, Mathf.Abs(_tempDir.y), _tempDir.z) * throwForce;
        Debug.DrawLine(lootLaunchPoint, lootLaunchPoint + _tempDir, Color.blue, 3f);
        _temp.GetComponent<Rigidbody>().AddForce(_tempDir, ForceMode.Impulse);
    }

    void HealthChanged(TruckModel model, float value)
    {
        _health = value;
    }

    void ForcesChanged(TruckModel model, Vector3 value)
    {
        _explosionForce = value;
    }

    #endregion
}


[Serializable]
public class TruckWheel
{
    public WheelCollider collider;
    public Transform model;
    public bool isPowered;
    public bool isSteeringWheel;
    public bool isReverseSteeringWheel;
}