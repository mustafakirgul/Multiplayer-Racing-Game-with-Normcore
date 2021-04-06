using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Normal.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class Truck : RealtimeComponent<TruckModel>
{
    public TruckWheel[] _wheels;
    public float maxTorque; //per powered wheel

    [Range(0f, 1f)] public float _torqueFactor;
    [Range(-20f, 20f)] public float _steeringAngle;
    public float maxSteeringAngle;
    public bool _handBrake;
    public Vector3 explosionPoint;
    [Space] [Header("Loot Settings")] public Vector3 lootLaunchPoint;
    [Range(0, 1)] public float lootChance;
    public float throwForce;

    [Space]
    //Truck Way Points System
    [SerializeField]
    private List<WayPoint> m_wayPoints;

    [SerializeField] private Transform currentWPT;
    [SerializeField] private float steerRefreshTimer = 1f / 60f;
    [SerializeField] private int currentWPindex;
    public float waypointSwitchThreshold;

    public GameObject damageSphere;
    public float damageDisplayTime;
    public bool isInvincible = true;

    //X-Ray silhouette
    public Renderer TruckMesh;
    public Outline TruckOutline;

    //Torque adjustment relative to elevation trend of the ground
    [SerializeField]
    private float angle, elevationTorqueFactor, elevationConstant, currentTorquePerWheel, torqueBoostAngleLimit;

    public bool isBoombastic;
    public float boombasticModeY = 25f;
    public float boombasticModeDuration = 33f;
    public ParticleSystem boombasticShield;
    public float groundCheckRayLength, lerpRotationSpeed, groundedEngineFactor;
    public LayerMask groundLayer;
    private int _length;
    private Transform _ownerTransform;
    private Vector3 _position;
    private Quaternion _rotation;
    private RealtimeTransform _rTTransform;
    private List<RealtimeTransform> _rTTransforms;

    private Vector3 _tempSizeForDamageSphere;
    private Vector3 boombasticModePoint = Vector3.zero;

    private Coroutine cr_DamageFeedback;
    private bool damageFeedback;
    private bool isGrounded;

    private bool postBoom;
    private SphereCollider shieldCollider;

    private int theKiller;
    private Rigidbody truckBody;
    private WaitForSeconds wait;

    private WaitForSeconds waitASecond;
    private Vector3 _startPosition => new Vector3(0, 30, 0);

    public RealtimeTransform rtTransform => GetComponent<RealtimeTransform>();
    private Rigidbody rb => GetComponent<Rigidbody>();

    private UIManager uIManager;

    private void Awake()
    {
        shieldCollider = boombasticShield.transform.GetComponent<SphereCollider>();
        if (shieldCollider != null) shieldCollider.enabled = false;
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
        uIManager = FindObjectOfType<UIManager>();
    }

    private void Start()
    {
        if (realtimeView.isOwnedRemotelyInHierarchy)
        {
            damageSphere.SetActive(false);
            return;
        }

        uIManager.LootTruckInvincibleIcon.SetActive(false);
        var temp = GameObject.FindGameObjectWithTag("boombasticPoint");
        if (temp != null) boombasticModePoint = temp.transform.position;
        StartHealth();
        InitializWaypointAI();
        waitASecond = new WaitForSeconds(steerRefreshTimer);
        if (damageSphere == null)
        {
            Debug.LogError("No damage sphere for truck!");
        }
        else
        {
            damageFeedback = true;
            damageSphere.SetActive(false);
        }

        for (var i = 0; i < _wheels.Length; i++)
        {
            _wheels[i].model.GetComponent<RealtimeView>().RequestOwnership();
            _wheels[i].model.GetComponent<RealtimeView>().RequestOwnership();
            _wheels[i].model.GetComponent<RealtimeTransform>().RequestOwnership();
        }
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
            GroundCheck();

            // if (Input.GetKeyDown(KeyCode.P))
            // {
            //     ResetTransform();
            // }

            //
            // if (Input.GetKeyDown(KeyCode.L)) // Drop Loot
            // {
            //     DropLoot();
            // }

            /*if (Input.GetKeyDown(KeyCode.K)) // Kill Ironhog
            {
                model.health = 0f;
            }*/

            if (transform.position.y < -300) ResetTransform();

            /*if (Input.GetKeyDown(KeyCode.Insert))
            {
                var pos = PlayerManager.instance.localPlayer.position;
                transform.position = new Vector3(pos.x, pos.y + 10f, pos.z);
            }*/

            currentTorquePerWheel = maxTorque * _torqueFactor * elevationTorqueFactor;
            if (_length > 0)
                for (var i = 0; i < _length; i++)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + lootLaunchPoint, 1f);
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, groundCheckRayLength,
            groundLayer);
        Debug.DrawLine(transform.position, transform.position + -transform.up * groundCheckRayLength, Color.cyan);

        if (!isGrounded)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.FromToRotation(transform.up, Vector3.up)
                * transform.rotation, Time.deltaTime * lerpRotationSpeed * 0.3f);
            groundedEngineFactor = 0f;
        }
        else
        {
            groundedEngineFactor = 1f;
        }
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

    public void SetInvincibility(bool state)
    {
        isInvincible = state;
    }

    public void InitializWaypointAI()
    {
        m_wayPoints = new List<WayPoint>();
        foreach (var waypoint in FindObjectsOfType<WayPoint>())
            if (waypoint.gameObject.activeInHierarchy && !m_wayPoints.Contains(waypoint))
                m_wayPoints.Add(waypoint);

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
                var _distanceToTarget = Vector3.Distance(transform.position, currentWPT.position);
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
        elevationTorqueFactor = 1f + Mathf.Clamp01(angle / torqueBoostAngleLimit) * groundedEngineFactor;
    }

    private void SetWayPoint(int wayPointIndex)
    {
        if (m_wayPoints.Count == 0)
            return;
        currentWPT = m_wayPoints[wayPointIndex % m_wayPoints.Count].transform;
    }

    private void CalculateRoute()
    {
        for (var i = 0; i < _length; i++)
        {
            _steeringAngle = maxSteeringAngle *
                             Vector3.Dot(
                                 Vector3.Cross(transform.forward,
                                     (currentWPT.position - transform.position).normalized), Vector3.up);

            if (_wheels[i].isSteeringWheel)
                _wheels[i].collider.steerAngle =
                    Mathf.Lerp(_wheels[i].collider.steerAngle, _steeringAngle, Time.deltaTime * 20f);

            if (_wheels[i].isReverseSteeringWheel)
                _wheels[i].collider.steerAngle =
                    Mathf.Lerp(_wheels[i].collider.steerAngle, -_steeringAngle, Time.deltaTime * 20f);
        }
    }

    private void ResetTransform()
    {
        transform.position = _startPosition;
        transform.rotation = Quaternion.identity;
    }

    private void BoombasticMode()
    {
        if (Vector3.Distance(rb.position, boombasticModePoint) > .1f)
        {
            rb.MovePosition(Vector3.Lerp(rb.position,
                boombasticModePoint, Time.deltaTime * 2f));
            transform.up = Vector3.Lerp(transform.up, Vector3.up, Time.deltaTime * 6.66f);
        }
        else
        {
            transform.Rotate(Vector3.up, Time.deltaTime * 5f);
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
            if (_explosionForce != _origin) ChangeExplosionForce(_origin);
        }
    }

    #region MODEL INTERACTIONS

    public float _health;
    public float _maxHealth;
    public float currentMaxHealth;
    public float scaleableHealth;
    public Vector3 _explosionForce;

    protected override void OnRealtimeModelReplaced(TruckModel previousModel, TruckModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.healthDidChange -= HealthChanged;
            previousModel.maxHealthDidChange -= MaxHealthChanged;
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
            currentModel.maxHealthDidChange += MaxHealthChanged;
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
        Handrake(value);
        uIManager.LootTruckInvincibleIcon.SetActive(value);

        if (value)
        {
            UpdateTorqueFactor(0f);
            StartCoroutine(CR_BackToNormal());
            boombasticShield.Play();
        }
        else
        {
            boombasticShield.Stop();
            UpdateTorqueFactor(1f);
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
        var numberOfPlayers = PlayerManager.instance.allPlayers.Length;
        GameManager.instance.GameStarted = true;
        model.maxHealth = scaleableHealth * numberOfPlayers;
        //_maxHealth = (scaleableHealth * numberOfPlayers);
        model.health = scaleableHealth * numberOfPlayers;
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

        if (_health < _maxHealth * .5f && !postBoom)
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

    private IEnumerator CR_DamageFeedback()
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
        if (Random.Range(0, 1f) < lootChance) //random chance of loot drop
            DropLoot();
    }

    private void DropLoot()
    {
        var _temp = Realtime.Instantiate("Loot",
            transform.position + lootLaunchPoint,
            Quaternion.identity,
            true,
            false,
            useInstance:
            realtime);
        GameManager.instance.RecordRIGO(_temp);
        var PUCount =
            LootManager.instance.playerLootPoolSave.PlayerPowerUps.Count - 1;
        var LootCount =
            LootManager.instance.playerLootPoolSave.m_RollPool.Count - 1;
        var RandomID = Random.Range(-PUCount, LootCount);
        //Debug.Log("PU id is" + RandomID);

        _temp.GetComponent<LootContainer>().SetID(RandomID);


        var _tempDir = Random.onUnitSphere;
        _tempDir = new Vector3(_tempDir.x, Mathf.Abs(_tempDir.y), _tempDir.z) * throwForce;
        Debug.DrawLine(lootLaunchPoint, lootLaunchPoint + _tempDir, Color.blue, 3f);
        _temp.GetComponent<Rigidbody>().AddForce(_tempDir, ForceMode.Impulse);
    }

    private void HealthChanged(TruckModel model, float value)
    {
        _health = value;
    }

    private void MaxHealthChanged(TruckModel model, float value)
    {
        _maxHealth = value;
    }

    private void ForcesChanged(TruckModel model, Vector3 value)
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