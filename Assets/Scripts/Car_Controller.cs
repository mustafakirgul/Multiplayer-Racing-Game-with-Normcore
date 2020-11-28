using UnityEngine;

public class Car_Controller : MonoBehaviour
{
    Rigidbody Rb => GetComponent<Rigidbody>();
    public float forwardThrust = 100f;
    public float rotationPower = 1f;
    public float brakePower = 1f;
    public float safeMaxHeight;
    float vertical, horizontal, velocity, sidewaysVelocity;
    bool isBreaking, isAtSafeHeight;
    RaycastHit hit;

    private void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        isBreaking = Input.GetKey(KeyCode.Space);
        velocity = Mathf.Abs(transform.InverseTransformVector(Rb.velocity).z);
        sidewaysVelocity = Mathf.Abs(transform.InverseTransformVector(Rb.velocity).x);
        isAtSafeHeight = Physics.Raycast(transform.position, -transform.up, out hit, safeMaxHeight);
        if (isAtSafeHeight)
            Debug.DrawLine(transform.position, transform.TransformVector(-transform.up) * safeMaxHeight, Color.green);
        else
            Debug.DrawLine(transform.position, transform.TransformVector(-transform.up) * safeMaxHeight, Color.red);
    }

    void FixedUpdate()
    {
        Rb.useGravity = !isAtSafeHeight;
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

        if (isAtSafeHeight)
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal);

        if (velocity != 0 || sidewaysVelocity != 0)
        {
            Rb.AddTorque(
                transform.up *
                Time.deltaTime *
                horizontal *
                rotationPower
            );
        }
    }
}
