using UnityEngine;

public class Car_Controller : MonoBehaviour
{
    Rigidbody Rb => GetComponent<Rigidbody>();
    public float forwardThrust = 100f;
    public float rotationPower = 1f;
    void Update()
    {
        Rb.AddForce(
            Input.GetAxisRaw("Vertical") * 
            transform.forward *
            Time.deltaTime *
            forwardThrust,
            ForceMode.Acceleration
        );
        Rb.AddTorque(0, Input.GetAxisRaw("Horizontal") * Time.deltaTime * rotationPower, 0, ForceMode.VelocityChange);
    }
}
