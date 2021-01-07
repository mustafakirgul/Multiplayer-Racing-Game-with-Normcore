using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCarController : MonoBehaviour
{
    [SerializeField]
    Rigidbody CarRB;

    private float moveInput, turnInput;

    public float fwdSpeed, reverseSpd, turnSpd;

    public float airDrag, groundDrag;

    public LayerMask groundLayer;

    public float rotationLerpSpeedforNormalBasedRotation;

    [SerializeField]
    private bool isGrounded;
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

            if (turnInput == 0)
            {
                RotationCheck();
            }
            else
            {
                float newRotation = turnInput * turnSpd * Time.fixedDeltaTime;
                CarRB.rotation = Quaternion.Euler(transform.rotation.x, newRotation, transform.rotation.z);
                //transform.Rotate(0, newRotation, 0, Space.World);
            }
        }
        else
        {
            //CarRB.AddForce(transform.up * -30f);
        }

    }

    private void RotationCheck()
    {
        Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, Mathf.Infinity, groundLayer);


        //transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal.normalized);

        //transform.up = Vector3.Lerp(transform.up, hit.normal, Time.fixedDeltaTime * rotationLerpSpeedforNormalBasedRotation);

        //(
        //Quaternion.Euler(Damp(transform.rotation.x,hit.point.x,rotationLerpSpeedforNormalBasedRotation,Time.fixedDeltaTime), 
        //transform.rotation.y,
        //Damp(transform.rotation.z, hit.point.z, rotationLerpSpeedforNormalBasedRotation, Time.fixedDeltaTime)
        //));

        CarRB.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, hit.normal), 1 - Mathf.Exp(-Time.fixedDeltaTime * rotationLerpSpeedforNormalBasedRotation));
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
