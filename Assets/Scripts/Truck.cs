using System;
using UnityEngine;

public class Truck : MonoBehaviour
{
    public TruckWheel[] _wheels;
    public float _torque;//per powered wheel
    [Range(0f, 1f)]
    public float _torqueFactor;
    [Range(-45f, 45f)]
    public float _steeringAngle;
    public bool _handBrake;
    int _length;
    Vector3 _position;
    Quaternion _rotation;

    private void Start()
    {
        _length = _wheels.Length;
    }

    private void Update()
    {
        if (_length > 0)
        {
            for (int i = 0; i < _length; i++)
            {
                if (_handBrake)
                    _wheels[i].collider.brakeTorque = _torque;
                else if (_wheels[i].isPowered)
                    _wheels[i].collider.motorTorque = _torque*_torqueFactor;

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
}
[Serializable]
public class TruckWheel
{
    public WheelCollider collider;
    public Transform model;
    public bool isPowered;
    public bool isSteeringWheel;
}
