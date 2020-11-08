using UnityEngine;

public class Truck : MonoBehaviour
{
    public Transform[] wheels;
    public float _torque;

    GameObject[] _temp;
    WheelCollider[] _wcs;
    int _length;
    Vector3 _position;
    Quaternion _rotation;

    private void Start()
    {
        _temp = GameObject.FindGameObjectsWithTag("TruckTire");
        _length = _temp.Length;
        wheels = new Transform[_length];
        _wcs = new WheelCollider[_length];
        for (int i = 0; i < _length; i++)
        {
            if (_temp[i].transform.parent == transform)
            {
                wheels[i] = _temp[i].transform.GetChild(0);
                _wcs[i] = _temp[i].transform.GetChild(1).GetComponent<WheelCollider>();
            }
        }
    }
    private void Update()
    {
        if (_length > 0)
        {
            for (int i = 0; i < _length; i++)
            {
                _wcs[i].motorTorque = _torque;
                _wcs[i].GetWorldPose(out _position, out _rotation);
                wheels[i].position = _position;
                wheels[i].rotation = _rotation;
            }
        }
    }
}
