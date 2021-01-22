﻿using System;
using UnityEngine;
using Normal.Realtime;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Random = System.Random;

public class Truck : RealtimeComponent<TruckModel>
{
    public TruckWheel[] _wheels;
    public float _torque; //per powered wheel
    [Range(0f, 1f)] public float _torqueFactor;
    [Range(-20f, 20f)] public float _steeringAngle;
    public float maxSteeringAngle;
    public bool _handBrake;
    int _length;
    Vector3 _position;
    Quaternion _rotation;
    Realtime _realtime;
    RealtimeTransform _rTTransform;
    List<RealtimeTransform> _rTTransforms;
    public Vector3 explosionPoint;
    Transform _ownerTransform;
    bool isNetworkInstance;
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
    [SerializeField] private float steerRefreshTimer = 1 / 60;
    [SerializeField] private int currentWPindex = 0;
    public float waypointSwitchThreshold;

    WaitForSeconds waitASecond;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + lootLaunchPoint, 1f);
    }

    private void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        truckBody = GetComponent<Rigidbody>();
        _rTTransform = GetComponent<RealtimeTransform>();
        _rTTransforms = new List<RealtimeTransform>();
        _rTTransforms.AddRange(GetComponentsInChildren<RealtimeTransform>());
        _rTTransforms.Add(GetComponent<RealtimeTransform>());
        _length = _wheels.Length;
        var centerOfMass = truckBody.centerOfMass;
        centerOfMass = new Vector3(centerOfMass.x, centerOfMass.y - 5, centerOfMass.z);
        truckBody.centerOfMass = centerOfMass;
    }

    private void Start()
    {
        StartHealth();
        InitializWaypointAI();
        waitASecond = new WaitForSeconds(steerRefreshTimer);
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
            while (currentWPT != null && !isNetworkInstance)
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
                yield return waitASecond;
            }

            yield return waitASecond;
        }
    }

    void SetWayPoint(int wayPointIndex)
    {
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
        //Debug.Log("truck health is: " + _health);

        isNetworkInstance = !_rTTransform.isOwnedLocallySelf;

#if (UNITY_EDITOR)
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetTransform();
        }
#endif
        if (transform.position.y < -300)
        {
            ResetTransform();
        }

        if (_length > 0)
        {
            for (int i = 0; i < _length; i++)
            {
                if (_handBrake)
                {
                    _wheels[i].collider.motorTorque = 0f;
                    _wheels[i].collider.brakeTorque = _torque;
                }
                else if (_wheels[i].isPowered)
                {
                    _wheels[i].collider.brakeTorque = 0f;
                    _wheels[i].collider.motorTorque = _torque * _torqueFactor;
                }

                if (_wheels[i].model.GetComponent<RealtimeTransform>().ownerIDSelf !=
                    GetComponent<RealtimeTransform>().ownerIDSelf)
                    _wheels[i].model.GetComponent<RealtimeTransform>()
                        .SetOwnership(GetComponent<RealtimeTransform>().ownerIDSelf);


                _wheels[i].collider.GetWorldPose(out _position, out _rotation);

                _wheels[i].model.position = _position;

                _wheels[i].model.rotation = _rotation;
            }
        }
    }

    public void AddExplosionForce(Vector3 _origin)
    {
        if (!isNetworkInstance)
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

    public int _owner;
    public float _health;
    public float _maxHealth;
    public Vector3 _explosionForce;

    protected override void OnRealtimeModelReplaced(TruckModel previousModel, TruckModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.ownerDidChange -= OwnerChanged;
            previousModel.healthDidChange -= HealthChanged;
            previousModel.explosionPointDidChange -= ForcesChanged;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
                StartHealth();
            currentModel.ownerDidChange += OwnerChanged;
            currentModel.healthDidChange += HealthChanged;
            currentModel.explosionPointDidChange += ForcesChanged;
        }
    }

    void SetOwner(int _id)
    {
        if (_id >= 0)
        {
            model.owner = _id;
        }
    }

    void ResetExplosionPoint()
    {
        model.explosionPoint = Vector3.zero;
    }

    void ChangeExplosionForce(Vector3 _origin)
    {
        model.explosionPoint += _origin;
    }

    public void StartHealth()
    {
        model.health = _maxHealth;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void DamagePlayer(float damage)
    {
        model.health -= damage;
        DropRandomLoot();
    }

    private void DropRandomLoot()
    {
        if (UnityEngine.Random.Range(0f, 1f) < lootChance) //random chance of loot drop
        {
            GameObject _temp = Realtime.Instantiate("Loot",
                position: transform.position + lootLaunchPoint,
                rotation: Quaternion.identity,
                ownedByClient:
                true,
                preventOwnershipTakeover:
                true,
                useInstance:
                _realtime);
            _temp.GetComponent<LootContainer>().SetID(UnityEngine.Random.Range(-666, 666));
            Vector3 _tempDir = UnityEngine.Random.onUnitSphere;
            _tempDir = new Vector3(_tempDir.x, Mathf.Abs(_tempDir.y), _tempDir.z) * throwForce;
            Debug.DrawLine(lootLaunchPoint, lootLaunchPoint + _tempDir, Color.blue, 3f);
            _temp.GetComponent<Rigidbody>().AddForce(_tempDir, ForceMode.Impulse);
        }
    }

    void HealthChanged(TruckModel model, float value)
    {
        _health = model.health;
    }

    void ForcesChanged(TruckModel model, Vector3 value)
    {
        _explosionForce = model.explosionPoint;
    }

    void OwnerChanged(TruckModel model, int value)
    {
        _owner = model.owner;
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