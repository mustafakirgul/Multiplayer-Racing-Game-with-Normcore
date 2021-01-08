using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCarController : MonoBehaviour
{
    [SerializeField]
    Rigidbody CarRB;

    private float moveInput, turnInput;
    public float fwdSpeed, reverseSpd, turnSpd, turningFwdSpeed;
    public float airDrag, groundDrag;

    public LayerMask groundLayer;
    public float lerpRotationSpeed;

    [SerializeField]
    private bool isGrounded;

    public ArcadeWheel[] wheels;
    // Start is called before the first frame update
    void Start()
    {
        CarRB.transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        DetectInput();
        DragCheck();
        GroundCheck();
        RotationCheck();

        transform.position = CarRB.transform.position;
    }
    void DetectInput()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        if (moveInput > 0)
        {
            moveInput *= fwdSpeed;
        }
        else
        {
            moveInput *= reverseSpd;
        }
    }

    void GroundCheck()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation), Time.deltaTime * lerpRotationSpeed);
    }


    void DragCheck()
    {
        if (isGrounded)
        {
            CarRB.drag = groundDrag;
        }
        else
        {
            CarRB.drag = airDrag;
        }
    }
    private void FixedUpdate()
    {
        if (isGrounded)
        {
            CarRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        }
        else
        {
            //Increase artifical gravity when in freefall
            CarRB.AddForce(transform.up * -30f);
        }
    }

    private void RotationCheck()
    {
        //transform.rotation = Quaternion.Euler(transform.rotation.x, newRotation, transform.rotation.z);
        //transform.Rotate(0, Mathf.Lerp(transform.rotation.eulerAngles.y, newRotation, Time.deltaTime), 0, Space.World);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, newRotation, 0), Time.deltaTime);

        //Debug.Log(" Vertical input is: " + moveInput);
        //if only pressing turning there should be a small amount of acceleration/speed so car is not turning on its own without momentum

        if (turnInput != 0 && moveInput == 0)
        {
            //Debug.Log("Stationary Turning");
            float StationaryRotation = turnInput * turnSpd * Time.deltaTime;
            CarRB.AddForce(transform.forward * turningFwdSpeed, ForceMode.Acceleration);
            transform.Rotate(0, StationaryRotation, 0, Space.World);
        }
        else
        {
            float motionRotation = turnInput * turnSpd * Time.deltaTime* Input.GetAxisRaw("Vertical");
            transform.Rotate(0, motionRotation, 0, Space.World);
        }        
    }
    
    public float _localVelovity()
    {
        Vector3 localVelocity = CarRB.transform.InverseTransformDirection(CarRB.velocity);
        return localVelocity.z;
    }

    //Framerate aware damping
    public static float Damp(float source, float target, float smoothing, float dt)
    {
        return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    }

    float CarAngle()
    {
        float carAngle = 0;
        Vector3 tangente = new Vector3();
        RaycastHit hit2;
        if (Physics.Raycast(CarRB.transform.position, -CarRB.transform.up, out hit2, 10, groundLayer))
        {
            hit2.normal.Normalize();
            var distance = -Vector3.Dot(hit2.normal, Vector3.up);
            //Debug.DrawRay(hit2.point, (Vector3.up + hit2.normal * distance).normalized , Color.white);
            tangente = (Vector3.up + hit2.normal * distance).normalized;
            carAngle = Vector3.Angle(tangente, -Vector3.up);                                                        // the current car angle
                                                                                                                    //Debug.DrawRay(hit2.point,  -Vector3.up, Color.cyan);
        }
        return carAngle;
    }
}
[Serializable]
public struct ArcadeWheel
{
    Transform t;
    bool isPowered;
    bool isSteeringWheel;
}
