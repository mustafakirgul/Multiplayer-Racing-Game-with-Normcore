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
    [SerializeField]
    public ArcadeWheel[] wheels;
    int wheelCount;
    Quaternion _tempQ;
    float _tempSuspensionDistance;
    public float maxSteeringAngle;
    public float wheelTurnFactor;
    public Transform carBody;
    public float maxZrotation, maxXRotation;
    public float zRotationLERPSpeed, xRotationLERPSpeed;
    public float rotationCooldownTime;
    float currentZ, currentX;
    float XTimer, ZTimer, XFactor, ZFactor;

    // Start is called before the first frame update
    void Start()
    {
        CarRB.transform.parent = null;
        wheelCount = wheels.Length;
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].originY = wheels[i].wheelT.localPosition.y;
        }
        currentX = carBody.localEulerAngles.x;
        currentZ = carBody.localEulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        DetectInput();
        DragCheck();
        GroundCheck();
        RotationCheck();
        TurnTheWheels();
        transform.position = CarRB.transform.position;
    }

    private void TurnTheWheels()
    {
        for (int i = 0; i < wheelCount; i++)
        {
            Debug.DrawRay(wheels[i].t.position, -wheels[i].t.up, Color.white);
            Physics.Raycast(wheels[i].t.position, -wheels[i].t.up, out RaycastHit hit, Mathf.Infinity, groundLayer);
            _tempSuspensionDistance = Mathf.Clamp(hit.distance, 0, wheels[i].suspensionHeight);
            wheels[i].trail.GetComponent<TrailRenderer>().emitting = hit.distance < wheels[i].suspensionHeight + wheels[i].wheelSize;

            Debug.DrawLine(wheels[i].t.position, hit.point, Color.red);

            wheels[i].wheelT.localPosition = new Vector3(wheels[i].wheelT.localPosition.x, wheels[i].originY - _tempSuspensionDistance + wheels[i].wheelSize, wheels[i].wheelT.localPosition.z);

            if (wheels[i].isSteeringWheel)
            {
                _tempQ = wheels[i].t.localRotation;
                wheels[i].wheelT.localEulerAngles = new Vector3(_tempQ.eulerAngles.x, turnInput * maxSteeringAngle, _tempQ.eulerAngles.z);
            }

            if (wheels[i].isPowered)
            {
                if (moveInput != 0)
                {
                    wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * moveInput * wheelTurnFactor);
                }
                else
                {
                    wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * LocalVelocity() * wheelTurnFactor);
                }
            }
            else
            {
                wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * LocalVelocity() * wheelTurnFactor);
            }
        }

    }
    void DetectInput()
    {
        if (moveInput != Input.GetAxis("Vertical") && moveInput == 0)
        {
            XTimer = rotationCooldownTime;
        }

        if (turnInput == 0 && Input.GetAxis("Horizontal") != turnInput)
        {
            ZTimer = rotationCooldownTime;
        }

        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");



        if (XTimer > 0f)
        {
            XTimer -= Time.deltaTime;
            XFactor = (XTimer / rotationCooldownTime)*maxXRotation;
            currentX = Mathf.Clamp(Mathf.LerpAngle(currentX, -moveInput * maxXRotation, xRotationLERPSpeed * Time.deltaTime), -XFactor, XFactor);

        }

        if (ZTimer > 0f)
        {
            ZTimer -= Time.deltaTime;
            ZFactor = (ZTimer / rotationCooldownTime) * maxZrotation;
            currentZ = Mathf.Clamp(Mathf.LerpAngle(currentZ, turnInput * maxZrotation, zRotationLERPSpeed * Time.deltaTime), -ZFactor, ZFactor);
        }


        carBody.localEulerAngles = new Vector3(currentX, carBody.localEulerAngles.y, currentZ);


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
        isGrounded = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 1f, groundLayer);
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
            float motionRotation = turnInput * turnSpd * Time.deltaTime * Input.GetAxisRaw("Vertical");
            transform.Rotate(0, motionRotation, 0, Space.World);
        }
    }

    public float LocalVelocity()
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
    public Transform t;//connection point to the car
    public Transform wheelT;//transform of actual wheel
    public GameObject trail;
    public bool isPowered;
    public bool isSteeringWheel;
    public float suspensionHeight;
    public float wheelSize;//radius of the wheel (r)
    public float originY;
}
