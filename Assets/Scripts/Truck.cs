using System;
using UnityEngine;
using Normal.Realtime;
using System.Collections.Generic;
using System.Collections;

public class Truck : RealtimeComponent<TruckModel>
{
    public TruckWheel[] _wheels;
    public float _torque;//per powered wheel
    [Range(0f, 1f)]
    public float _torqueFactor;
    [Range(-20f, 20f)]
    public float _steeringAngle;
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

    //Truck Way Points System
    [SerializeField]
    private List<Transform> m_wayPoints;
    [SerializeField]
    private Transform currentWP;
    [SerializeField]
    private float steerRefreshTimer;
    [SerializeField]
    private int currentWPindex = 0;

    private void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        truckBody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        _rTTransform = GetComponent<RealtimeTransform>();
        _rTTransforms = new List<RealtimeTransform>();
        _rTTransforms.AddRange(GetComponentsInChildren<RealtimeTransform>());
        _rTTransforms.Add(GetComponent<RealtimeTransform>());
        _length = _wheels.Length;

        ResetWayPoints();
    }

    public void ResetWayPoints()
    {
        //Find all waypoints with the waypoints tag
        for (int i = (FindObjectsOfType<WayPoint>().Length-1); i > -1; i--)
        {
            m_wayPoints.Add(FindObjectsOfType<WayPoint>()[i].transform);
        }

        currentWP = m_wayPoints[0];
        SetWayPointDirection(currentWP);
        StartCoroutine(CheckSteering());
    }
    void ResetTransform()
    {
        transform.position = _startPosition;
        transform.rotation = Quaternion.identity;
    }
    private void Update()
    {
        if (_ownerTransform == null)
        {
            _ownerTransform = PlayerManager.instance.RequestOwner(_rTTransforms);
        }

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

                if (_wheels[i].model.GetComponent<RealtimeTransform>().ownerIDSelf != GetComponent<RealtimeTransform>().ownerIDSelf)
                    _wheels[i].model.GetComponent<RealtimeTransform>().SetOwnership(GetComponent<RealtimeTransform>().ownerIDSelf);

                if (_wheels[i].isSteeringWheel)
                {
                    _wheels[i].collider.steerAngle = _steeringAngle;
                }

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
    private TruckModel _truck;
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
                _maxHealth = currentModel.health;
            _health = currentModel.health;
            _owner = currentModel.owner;
            _explosionForce = currentModel.explosionPoint;
            currentModel.ownerDidChange += OwnerChanged;
            currentModel.healthDidChange += HealthChanged;
            currentModel.explosionPointDidChange += ForcesChanged;
            _truck = currentModel;
        }
    }

    private IEnumerator CheckSteering()
    {
        while (true)
        {
            CheckWayPointTarget(currentWP, 2f);
            yield return new WaitForSeconds(steerRefreshTimer);
        }
    }

    void CheckWayPointTarget(Transform currentWaypoint, float distanceThreshold)
    {
        if (Vector3.Distance(this.transform.position, currentWaypoint.position) < distanceThreshold)
        {
            currentWPindex++;
            SetWayPoint(currentWPindex);
        }
        else
        {
            SetWayPoint(currentWPindex);
            return;
        }
    }

    void SetWayPoint(int wayPointIndex)
    {
        if (wayPointIndex > m_wayPoints.Count)
        {
            currentWPindex = wayPointIndex % m_wayPoints.Count;
            currentWP = m_wayPoints[currentWPindex];
            SetWayPointDirection(currentWP);
            Debug.Log("spill over going to" + currentWPindex);
    
        }
        else
        {
            currentWP = m_wayPoints[wayPointIndex];
            SetWayPointDirection(currentWP);
            Debug.Log("Going to" + wayPointIndex);
        }
    }

    void SetWayPointDirection(Transform WP)
    {
        _steeringAngle = Mathf.Clamp((Quaternion.FromToRotation(transform.forward, WP.transform.position - this.transform.position).y * 50f), -45, 45); ;
        
        Debug.Log("Current Steering Angle is" + _steeringAngle);
    }

    void SetOwner(int _id)
    {
        if (_id >= 0)
        {
            _truck.owner = _id;
        }
    }
    void ResetExplosionPoint()
    {
        _truck.explosionPoint = Vector3.zero;
    }
    void ChangeExplosionForce(Vector3 _origin)
    {
        _truck.explosionPoint += _origin;
    }

    void DamagePlayer(float damage)
    {
        HealthChanged(_truck, (_health - damage));
    }

    void HealthChanged(TruckModel model, float value)
    {
        _health = value;
    }

    void ForcesChanged(TruckModel model, Vector3 value)
    {
        _explosionForce = value;
    }

    void OwnerChanged(TruckModel model, int value)
    {
        _owner = value;
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
}
