using System;
using UnityEngine;

public class Car_Controller : MonoBehaviour
{
    Rigidbody Rb => GetComponent<Rigidbody>();
    public float forwardThrust = 100f;
    public float rotationPower = 1f;
    public float brakePower = 1f;
    public float rotationDampener;
    public float downForce; // when above max safe height
    public float downForceHeight; // when downforce needs to be applied
    public BasicWheel[] wheels;
    public float maxSteering;
    [Space] public GameObject leftHeadLight;
    public GameObject rightHeadLight;
    float vertical, horizontal, velocity, trueVelocity, sidewaysVelocity, powerPerWheel;
    bool isBreaking, lights, applyDownforce, inReverse;
    int poweredWheelCount;
    Transform _temp;
    RaycastHit hit, hit2;

    private void Start()
    {
        if (!lights)
            lights = true;
        ToggleLights();
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i]._isPowered)
            {
                poweredWheelCount++;
            }
        }

        powerPerWheel = forwardThrust / poweredWheelCount;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            _temp = wheels[i]._transform;
            if (_temp != null)
            {
                Gizmos.DrawWireSphere(_temp.position, wheels[i]._radius);
            }
        }
    }

    private void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        isBreaking = Input.GetKey(KeyCode.Space) || (Mathf.Abs(vertical) < .1f && velocity < .25f);
        trueVelocity = transform.InverseTransformVector(Rb.velocity).z;
        inReverse = vertical < 0f;
        velocity = Mathf.Abs(trueVelocity);
        sidewaysVelocity = Mathf.Abs(transform.InverseTransformVector(Rb.velocity).x);
        applyDownforce = !Physics.Raycast(transform.position, -transform.up, out hit2, downForceHeight);

        if (applyDownforce)
            Debug.DrawLine(transform.position, transform.position - transform.up * downForceHeight, Color.blue);
        else
            Debug.DrawLine(transform.position, transform.position - transform.up * downForceHeight, Color.yellow);

        if (Input.GetKeyDown(KeyCode.R)) //reset
        {
            //transform.position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
            Vector3 _rotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
            Rb.velocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.E)) //lights
        {
            ToggleLights();
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i]._isSteeringWheel)
                wheels[i]._transform.localRotation = Quaternion.Euler(new Vector3(0, maxSteering * horizontal, 0));
            if (wheels[i]._isPowered && vertical != 0)
                wheels[i]._transform.Rotate(wheels[i]._transform.right, forwardThrust * vertical);
            else
                wheels[i]._transform.Rotate(wheels[i]._transform.right, trueVelocity / wheels[i]._circumference * 360f);
            wheels[i]._isGrounded = Physics.Raycast(wheels[i]._transform.position, -wheels[i]._transform.up, out hit,
                wheels[i]._radius);
        }
    }

    private void ToggleLights()
    {
        lights = !lights;
        rightHeadLight.SetActive(lights);
        leftHeadLight.SetActive(lights);
    }

    void FixedUpdate()
    {
        if (isBreaking)
        {
            if (velocity > 0 || sidewaysVelocity > 0)
            {
                Rb.velocity = Vector3.Lerp(Rb.velocity, Vector3.zero, brakePower);
                Rb.angularVelocity = Vector3.Lerp(Rb.angularVelocity, Vector3.zero, brakePower);
            }
        }
        else
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i]._isGrounded)
                {
                    Rb.AddForceAtPosition(
                        vertical *
                        transform.forward *
                        Time.deltaTime *
                        powerPerWheel,
                        wheels[i]._transform.position,
                        ForceMode.Acceleration
                    );
                }
            }
        }

        if (velocity > 0.1f) // reverse steering while driving backwards
        {
            if (inReverse)
                Rb.AddRelativeTorque(transform.up * -horizontal * rotationPower * Time.deltaTime);
            else
                Rb.AddRelativeTorque(transform.up * horizontal * rotationPower * Time.deltaTime);
        }

        if (applyDownforce)
        {
            Rb.AddForce(-Vector3.up * downForce);
        }
    }

    [Serializable]
    public class BasicWheel
    {
        public Transform _transform;
        public float _radius;
        public bool _isPowered;
        public bool _isSteeringWheel;
        [HideInInspector] public float _circumference;
        public bool _isGrounded;

        public BasicWheel(Transform transform, float radius)
        {
            _transform = transform;
            _radius = radius;
            _circumference = 2.0f * Mathf.PI * radius;
        }
    }
}