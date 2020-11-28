using System;
using UnityEngine;

public class Car_Controller : MonoBehaviour
{
    Rigidbody Rb => GetComponent<Rigidbody>();
    public float forwardThrust = 100f;
    public float rotationPower = 1f;
    public float brakePower = 1f;
    public float safeMaxHeight;
    public float rotationDampener;
    public float downForce; // when above max safe height
    public float downForceHeight; // when downforce needs to be applied
    public float upForce; // to lift the car up
    [Space]
    public GameObject leftHeadLight;
    public GameObject rightHeadLight;
    float vertical, horizontal, velocity, trueVelocity, sidewaysVelocity;
    bool isBreaking, isAtSafeHeight, lights, applyDownforce, inReverse;
    RaycastHit hit, hit2;

    private void Start()
    {
        if (!lights)
            lights = true;
        ToggleLights();
    }

    private void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        isBreaking = Input.GetKey(KeyCode.Space) || (Mathf.Abs(vertical) < .1f && velocity < .25f);
        trueVelocity = transform.InverseTransformVector(Rb.velocity).z;
        inReverse = trueVelocity < 0f;
        velocity = Mathf.Abs(trueVelocity);
        sidewaysVelocity = Mathf.Abs(transform.InverseTransformVector(Rb.velocity).x);
        isAtSafeHeight = Physics.Raycast(transform.position, -transform.up, out hit, safeMaxHeight);
        applyDownforce = !Physics.Raycast(transform.position, -transform.up, out hit2, downForceHeight);
        if (isAtSafeHeight)
            Debug.DrawLine(transform.position, transform.position - transform.up * safeMaxHeight, Color.green);
        else
            Debug.DrawLine(transform.position, transform.position - transform.up * safeMaxHeight, Color.red);

        if (applyDownforce)
            Debug.DrawLine(transform.position, transform.position - transform.up * downForceHeight, Color.blue);
        else
            Debug.DrawLine(transform.position, transform.position - transform.up * downForceHeight, Color.yellow);

        if (Input.GetKeyDown(KeyCode.R))//reset
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
            Vector3 _rotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
            Rb.velocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.E))//lights
        {
            ToggleLights();
        }

        if (velocity > 0.1f)// reverse steering while driving backwards
        {
            if (inReverse)
                transform.Rotate(transform.up, -horizontal * rotationPower * Time.deltaTime, Space.Self);
            else
                transform.Rotate(transform.up, horizontal * rotationPower * Time.deltaTime, Space.Self);
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
        else if (isAtSafeHeight)
        {
            Rb.AddForce(
                vertical *
                transform.forward *
                Time.deltaTime *
                forwardThrust,
                ForceMode.Acceleration
            );
        }

        if (applyDownforce)
        {
            Rb.AddForce(-Vector3.up * downForce);
        }
        else
        {
            Rb.AddForce(Vector3.up * upForce);
        }
    }
}
