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
        DectectInput();
        DragCheck();
        GroundCheck();
        transform.position = CarRB.transform.position;
        float newRotation = turnInput * turnSpd * Time.deltaTime;
        transform.Rotate(0, newRotation, 0, Space.World);
    }

    void DectectInput()
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

        transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

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
            CarRB.AddForce(transform.up * -30f);
        }
    }
}
